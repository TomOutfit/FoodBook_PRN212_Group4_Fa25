pipeline {
    agent any

    options {
        // Keeps the last 30 successful builds
        buildDiscarder(logRotator(numToKeepStr: '30'))
        timestamps()
        ansiColor('xterm')
        timeout(time: 30, unit: 'MINUTES')
    }

    environment {
        DOTNET_VERSION = '9.0'
        SOLUTION_PATH = 'CookBook.sln'
        TEST_PROJECT_PATH = 'Foodbook.Tests/Foodbook.Tests.csproj'
        TEST_RESULTS_DIR = 'TestResults'
        COVERAGE_DIR = 'CoverageReports'
        BUILD_CONFIG = 'Release'

        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
        
        NUGET_HTTP_TIMEOUT = '300'
        NUGET_PLUGIN_HANDSHAKE_TIMEOUT_IN_SECONDS = '120'
    }

    stages {
        stage('Checkout SCM') {
            steps {
                echo "⬇️ Checking out source code..."
                script {
                    checkout([$class: 'GitSCM',
                        branches: [[name: 'main']],
                        extensions: [[$class: 'CleanBeforeCheckout']],
                        userRemoteConfigs: [[
                            url: 'https://github.com/TomOutfit/FoodBook_PRN212_Group4_Fa25.git'
                        ]]
                    ])
                }
            }
        }

        stage('Clean') {
            steps {
                echo "🧹 Cleaning workspace and creating result directories..."
                bat '''
                    dotnet clean "%SOLUTION_PATH%" --configuration "%BUILD_CONFIG%" --verbosity minimal
                    if exist "%TEST_RESULTS_DIR%" rmdir /S /Q "%TEST_RESULTS_DIR%"
                    if exist "%COVERAGE_DIR%" rmdir /S /Q "%COVERAGE_DIR%"
                    mkdir "%TEST_RESULTS_DIR%"
                    mkdir "%COVERAGE_DIR%"
                '''
            }
        }

        stage('Restore') {
            steps {
                echo "📦 Restoring NuGet packages and installing tools..."
                retry(3) {
                    bat '''
                        dotnet --info
                        dotnet nuget locals all --clear
                        
                        rem *** ADDED: Install ReportGenerator tool ***
                        dotnet tool install --global dotnet-reportgenerator-globaltool --ignore-failed-sources
                        
                        rem Restore solution packages with stability flags
                        dotnet restore "%SOLUTION_PATH%" ^
                            --verbosity minimal ^
                            --no-cache ^
                            --disable-parallel ^
                            --source https://api.nuget.org/v3/index.json
                    '''
                }
            }
        }

        stage('Build') {
            steps {
                echo "🏗️ Building solution..."
                bat 'dotnet build "%SOLUTION_PATH%" --configuration "%BUILD_CONFIG%" --no-restore --verbosity minimal'
            }
        }

        stage('Unit Tests') {
            steps {
                echo "🧪 Running unit tests and collecting coverage..."
                // Dùng lệnh dotnet test với cờ CollectCoverage để tạo ra TRX và Cobertura XML
                bat '''
                    dotnet test "%TEST_PROJECT_PATH%" ^
                        --configuration "%BUILD_CONFIG%" ^
                        --no-build ^
                        --logger "trx;LogFileName=TestResults.trx" ^
                        --results-directory "%TEST_RESULTS_DIR%" ^
                        /p:CollectCoverage=true ^
                        /p:CoverletOutput="%TEST_RESULTS_DIR%/" ^
                        /p:CoverletOutputFormat=cobertura
                '''
            }
            post {
                always {
                    echo "📄 Publishing test results to JUnit reporter..."
                    // Publish TRX files. JUnit plugin sẽ đọc và tạo Tab Test Result
                    junit allowEmptyResults: true, testResults: "${TEST_RESULTS_DIR}/*.trx"
                }
            }
        }

        stage('Code Coverage') {
            steps {
                echo "📊 Publishing coverage report (Jenkins UI Integration)..."
                script {
                    // *** ĐÃ FIX: Chuyển logic PowerShell sang bước powershell riêng biệt ***
                    // 1. Tìm file cobertura.xml trong thư mục con của TestResults và copy ra
                    powershell '''
                        $sourceFile = Get-ChildItem -Recurse -Path 'TestResults' -Filter 'coverage.cobertura.xml' | Select-Object -First 1;
                        if ($sourceFile) {
                            Write-Host '✅ Tìm thấy coverage file: ' $sourceFile.FullName;
                            Copy-Item $sourceFile.FullName 'CoverageReports/coverage.cobertura.xml' -Force
                        } else {
                            Write-Host '⚠️ Không tìm thấy coverage file'
                        }
                    '''

                    def finalCoberturaFile = "${COVERAGE_DIR}/coverage.cobertura.xml" 
                    if (fileExists(finalCoberturaFile)) {
                        // 2. Publish using the Cobertura plugin (Adds the Coverage graph to Jenkins)
                        publishCoverage adapters: [
                            coberturaAdapter(finalCoberturaFile)
                        ], sourceFileResolver: sourceFiles('STORE_LAST_BUILD')
                    } else {
                        echo "⚠️ Không tìm thấy file coverage tại ${finalCoberturaFile} — bỏ qua bước này."
                    }
                }
            }
        }
        
        stage('Generate & Publish HTML Report') {
            steps {
                echo "📄 Generating and publishing HTML coverage report..."
                script {
                    def finalCoberturaFile = "${COVERAGE_DIR}/coverage.cobertura.xml"
                    if (fileExists(finalCoberturaFile)) {
                        // 1. Run ReportGenerator to create HTML report from Cobertura XML
                        bat '''
                            reportgenerator ^
                                -reports:"%COVERAGE_DIR%/coverage.cobertura.xml" ^
                                -targetdir:"%COVERAGE_DIR%/HtmlReport" ^
                                -reporttypes:Html
                        '''
                        
                        // 2. Publish the generated HTML report using HTML Publisher Plugin
                        echo "📊 Publishing HTML report to Jenkins..."
                        publishHTML(
                            target: [
                                allowMissing: false,
                                directory: "${COVERAGE_DIR}/HtmlReport",
                                indexPages: 'index.html',
                                keepAll: true,
                                reportName: 'HTML Coverage Report'
                            ]
                        )
                    } else {
                        echo "⚠️ Không tìm thấy file coverage XML, bỏ qua HTML report."
                    }
                }
            }
        }

        stage('Publish Artifacts') {
            steps {
                echo "🚀 Preparing build artifacts for archiving..."
                bat 'dotnet publish "%SOLUTION_PATH%" --configuration "%BUILD_CONFIG%" --no-build --output "%COVERAGE_DIR%/publish"'
            }
        }
        
        stage('Test Report Summary') {
            steps {
                echo '📋 Đang tạo Test Report Summary...'
                // *** ĐÃ FIX: Chuyển sang bước powershell riêng biệt để tránh lỗi Groovy ***
                powershell '''
                    $summaryFile = 'TestResults/test-summary.txt';
                    
                    $buildNumber = $env:BUILD_NUMBER;
                    $branchName = $env:GIT_BRANCH;
                    $commitId = $env:GIT_COMMIT;

                    $trx = @(Get-ChildItem -Path 'TestResults' -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue).Count;
                    $hasCov = Test-Path 'CoverageReports/coverage.cobertura.xml';
                    $hasCovText = if ($hasCov) { 'Yes' } else { 'No' };
                    
                    New-Item -ItemType Directory -Force -Path 'TestResults' | Out-Null;

                    Set-Content -Path $summaryFile -Value '╔══════════════════════════════════════════════════════════════╗' -Encoding UTF8;
                    Add-Content -Path $summaryFile -Value '║           📊 BÁO CÁO KẾT QUẢ TEST CASE - COOKBOOK             ║';
                    Add-Content -Path $summaryFile -Value '╚══════════════════════════════════════════════════════════════╝';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value "Ngày chạy: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')";
                    Add-Content -Path $summaryFile -Value "Build Number: $buildNumber";
                    Add-Content -Path $summaryFile -Value "Branch: $branchName";
                    Add-Content -Path $summaryFile -Value "Commit: $commitId";
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '───────────────────────────────────────────────────────────────';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '📈 TỔNG QUAN KẾT QUẢ:';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value "Số file TRX: $trx";
                    Add-Content -Path $summaryFile -Value 'Lưu ý: Nếu không có Test Case, vẫn coi là PASSED (0 test).';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '───────────────────────────────────────────────────────────────';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '📁 CÁC FILE BÁO CÁO:';
                    Add-Content -Path $summaryFile -Value '- Test Results (TRX): TestResults/*.trx';
                    Add-Content -Path $summaryFile -Value '- Test Results (JUnit): TestResults/*.xml';
                    Add-Content -Path $summaryFile -Value "- Code Coverage: CoverageReports/coverage.cobertura.xml (Available: $hasCovText)";
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '───────────────────────────────────────────────────────────────';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '🔗 XEM CHI TIẾT:';
                    Add-Content -Path $summaryFile -Value "- Test Results: Xem tab 'Test Result' bên trái";
                    Add-Content -Path $summaryFile -Value "- Code Coverage: Xem tab 'Coverage Report' (nếu có) bên trái";
                    Add-Content -Path $summaryFile -Value "- HTML Report: Xem liên kết 'HTML Coverage Report' bên trái (nếu có)";
                    Add-Content -Path $summaryFile -Value "- Console Output: Xem 'Console Output' để xem log chi tiết";
                    Add-Content -Path $summaryFile -Value '';
                    Get-Content $summaryFile | Write-Output
                '''
            }
        }
    }

    post {
        always {
            echo "📦 Archiving test results and coverage data..."
            archiveArtifacts artifacts: "${TEST_RESULTS_DIR}/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "${COVERAGE_DIR}/publish/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "${COVERAGE_DIR}/HtmlReport/**/*", allowEmptyArchive: true, fingerprint: true
        }

        success {
            echo "✅ BUILD SUCCESSFUL — All tests passed!"
        }

        failure {
            echo "❌ BUILD FAILED — Check console log for details."
        }

        unstable {
            echo "⚠️ BUILD KHÔNG ỔN ĐỊNH (Cảnh báo hoặc test skip)."
        }
    }
}
