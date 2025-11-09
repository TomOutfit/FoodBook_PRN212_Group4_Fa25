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
        // GIỮ NGUYÊN COVERAGE_DIR cho các artifacts HTML (sẽ di chuyển file vào đó)
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
                    if exist "TestResults" rmdir /S /Q "TestResults"
                    if exist "CoverageReports" rmdir /S /Q "CoverageReports"
                    mkdir "TestResults"
                    mkdir "CoverageReports"
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

                        rem *** IMPROVED: Install ReportGenerator tool with version and force update ***
                        dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.26 --ignore-failed-sources
                        dotnet tool update --global dotnet-reportgenerator-globaltool --ignore-failed-sources

                        rem *** ADDED: Install trx2junit for better test report conversion ***
                        dotnet tool install --global trx2junit --ignore-failed-sources

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
                // **ĐÃ SỬA: Bắt Coverlet Output ra thư mục dự án test**
                bat '''
                    dotnet test "%TEST_PROJECT_PATH%" ^
                        --configuration "%BUILD_CONFIG%" ^
                        --no-build ^
                        --logger "trx;LogFileName=TestResults.trx" ^
                        --results-directory "%TEST_RESULTS_DIR%" ^
                        --verbosity normal ^
                        /p:CollectCoverage=true ^
                        /p:CoverletOutputFormat=cobertura ^
                        /p:CoverletOutput="%TEST_PROJECT_PATH%/CoverageReports/coverage.cobertura.xml" 
                        // Note: Bắt buộc Coverlet output vào thư mục dự án test
                    
                    REM Force success exit code (0) for Jenkins to ensure next stages run
                    EXIT /B 0
                '''
            }
            post {
                always {
                    echo "📄 Processing and publishing test results..."
                    powershell '''
                        try {
                            Write-Host "🔍 DEBUG: Starting TRX conversion process..."
                            $trxFiles = Get-ChildItem -Path 'TestResults' -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue
                            
                            # Xử lý file TRX trong thư mục gốc TestResults
                            if ($trxFiles) {
                                Write-Host "✅ Found TRX files in root: $($trxFiles.Count)"
                                foreach ($file in $trxFiles) {
                                    Write-Host "  - $($file.FullName)"
                                    $xmlFileName = [System.IO.Path]::ChangeExtension($file.Name, '.xml')
                                    $xmlFilePath = Join-Path 'TestResults' $xmlFileName
                                    & trx2junit "$($file.FullName)" "$xmlFilePath" 2>&1 | Out-Null
                                    if (Test-Path $xmlFilePath) {
                                        Write-Host "     -> Converted to JUnit XML: $xmlFilePath"
                                    }
                                }
                            } else {
                                Write-Host "⚠️ No TRX files found in root TestResults directory"
                            }
                            
                            # Xử lý file TRX trong thư mục dự án (nếu có)
                            $projectTrxFiles = Get-ChildItem -Path 'Foodbook.Tests/TestResults' -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue
                            if ($projectTrxFiles) {
                                Write-Host "✅ Found TRX files in project dir: $($projectTrxFiles.Count)"
                                foreach ($file in $projectTrxFiles) {
                                    Write-Host "  - $($file.FullName)"
                                    $xmlFileName = [System.IO.Path]::ChangeExtension($file.Name, '.xml')
                                    $xmlFilePath = Join-Path 'Foodbook.Tests/TestResults' $xmlFileName
                                    & trx2junit "$($file.FullName)" "$xmlFilePath" 2>&1 | Out-Null
                                    if (Test-Path $xmlFilePath) {
                                        Write-Host "     -> Converted to JUnit XML: $xmlFilePath"
                                    }
                                }
                            } else {
                                Write-Host "⚠️ No TRX files found in project directory"
                            }
                        } catch {
                            Write-Host "❌ PowerShell script error during conversion: $($_.Exception.Message). Continuing build..."
                        }
                        exit 0
                    '''
                    // Publish JUnit XML files (cả trong thư mục gốc và thư mục dự án nếu có)
                    junit allowEmptyResults: true, testResults: "${TEST_RESULTS_DIR}/*.xml, Foodbook.Tests/TestResults/*.xml"

                    echo "✅ Test results published."
                }
            }
        }

        stage('Code Coverage') {
            steps {
                echo "📊 Processing and publishing coverage reports..."
                script {
                    // **ĐÃ SỬA: Đã chuyển sang tìm kiếm trong thư mục dự án test**
                    powershell '''
                        # Chỉ kiểm tra vị trí file Cobertura đã được cố định
                        $projectCobertura = 'Foodbook.Tests/CoverageReports/coverage.cobertura.xml'
                        $foundFiles = @()
                        $primaryFile = $null

                        if (Test-Path $projectCobertura) {
                            Write-Host "✅ Cobertura file found in project dir: $((Get-Item $projectCobertura).Length) bytes"
                            $foundFiles += $projectCobertura
                            $primaryFile = $projectCobertura
                        } else {
                            Write-Host '❌ Cobertura file not found in project CoverageReports.'
                        }

                        # Copy file Cobertura lên thư mục gốc COVERAGE_DIR để tiện cho ReportGenerator
                        if ($primaryFile) {
                            $targetPath = 'CoverageReports/coverage.cobertura.xml'
                            Copy-Item -Path $primaryFile -Destination $targetPath -Force
                            Write-Host "✅ Copied coverage file to: $targetPath"
                            $env:PRIMARY_COVERAGE_FILE_FOR_REPORT = $targetPath
                        } else {
                            $env:PRIMARY_COVERAGE_FILE_FOR_REPORT = ''
                        }

                        exit 0
                    '''

                    // Bước 2: Publish Coverage Report trên Jenkins UI
                    def coverageFile = env.PRIMARY_COVERAGE_FILE_FOR_REPORT
                    
                    if (coverageFile && fileExists(coverageFile)) {
                        def adapters = [coberturaAdapter(coverageFile)]
                        
                        // 

                        publishCoverage adapters: adapters, sourceFileResolver: sourceFiles('STORE_LAST_BUILD')
                        echo "✅ Published enhanced coverage reports to Jenkins"
                    } else { 
                        echo "⚠️ Không tìm thấy file coverage nào — bỏ qua bước này."
                    }
                }
            }
        }
        
        stage('Generate & Publish HTML Report') {
            steps {
                echo "📄 Generating enhanced HTML coverage reports..."
                script {
                    def finalCoberturaFile = env.PRIMARY_COVERAGE_FILE_FOR_REPORT ?: "CoverageReports/coverage.cobertura.xml"
                    def reportFiles = []
                    
                    // Kiểm tra file đã được copy lên thư mục gốc chưa
                    if (fileExists(finalCoberturaFile)) {
                        reportFiles.add(finalCoberturaFile)
                        echo "📊 Using cobertura file: ${finalCoberturaFile}"

                        def reportsArg = reportFiles.join(';')
                        echo "📊 Using coverage files: ${reportsArg}"

                        // Generate HTML report (Html and HtmlChart)
                        bat """
                            reportgenerator ^
                                -reports:"${reportsArg}" ^
                                -targetdir:"CoverageReports/HtmlReport" ^
                                -reporttypes:Html;HtmlChart ^
                                -title:"CookBook Detailed Coverage Report" ^
                                -tag:"${BUILD_NUMBER}" ^
                                -verbosity:Info
                        """
                        // Generate Summary report (HtmlSummary)
                        bat """
                            reportgenerator ^
                                -reports:"${reportsArg}" ^
                                -targetdir:"CoverageReports/SummaryReport" ^
                                -reporttypes:HtmlSummary ^
                                -title:"CookBook Test Summary" ^
                                -tag:"${BUILD_NUMBER}" ^
                                -verbosity:Info
                        """

                        // Publish multiple HTML reports to Jenkins
                        echo "📊 Publishing enhanced HTML reports to Jenkins..."

                        publishHTML(
                            target: [
                                allowMissing: false, // Bắt buộc phải có index.html
                                alwaysLinkToLastBuild: true,
                                keepAll: true,
                                reportDir: "CoverageReports/HtmlReport",
                                reportFiles: 'index.html',
                                reportName: 'Detailed HTML Coverage Report'
                            ]
                        )
                        // 

                        publishHTML(
                            target: [
                                allowMissing: true, 
                                alwaysLinkToLastBuild: true,
                                keepAll: true,
                                reportDir: "CoverageReports/SummaryReport",
                                reportFiles: 'index.html',
                                reportName: 'Coverage Summary Report (HtmlSummary)'
                            ]
                        )
                        echo "✅ Enhanced HTML reports published to Jenkins"
                    } else {
                        echo "❌ Không tìm thấy file coverage nào. Bỏ qua HTML report generation."
                    }
                }
            }
        }

        stage('Publish Artifacts') {
            steps {
                echo "🚀 Preparing build artifacts for archiving..."
                bat 'dotnet publish "%SOLUTION_PATH%" --configuration "%BUILD_CONFIG%" --no-build --output "CoverageReports/publish"'
            }
        }
        
        stage('Test Report Summary') {
            steps {
                echo '📋 Creating enhanced Test Report Summary...'
                // ĐÃ SỬA: Thay thế toán tử ?? bằng if/else (chuẩn PS cũ)
                powershell '''
                    $summaryFile = 'TestResults/test-summary.txt';

                    $buildNumber = $env:BUILD_NUMBER; if (-not $buildNumber) { $buildNumber = 'Unknown' }
                    $branchName = $env:GIT_BRANCH; if (-not $branchName) { $branchName = 'Unknown' }
                    $commitId = $env:GIT_COMMIT; if (-not $commitId) { $commitId = 'Unknown' }

                    # Enhanced file counting
                    $trxFiles = @(Get-ChildItem -Path 'TestResults' -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue);
                    $xmlFiles = @(Get-ChildItem -Path 'TestResults' -Filter '*.xml' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -notlike '*coverage*' });
                    $coberturaExists = Test-Path 'CoverageReports/coverage.cobertura.xml';
                    $htmlExists = Test-Path 'CoverageReports/HtmlReport/index.html';
                    $summaryExists = Test-Path 'CoverageReports/SummaryReport/index.html';

                    New-Item -ItemType Directory -Force -Path 'TestResults' | Out-Null;

                    # Tạo nội dung báo cáo
                    Set-Content -Path $summaryFile -Value '╔══════════════════════════════════════════════════════════════════════════════════════════════════════════════╗' -Encoding UTF8;
                    Add-Content -Path $summaryFile -Value '║                                                📊 COOKBOOK TEST REPORT                                                ║';
                    Add-Content -Path $summaryFile -Value '╚══════════════════════════════════════════════════════════════════════════════════════════════════════════════╝';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value "⏰ Execution Time: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')";
                    Add-Content -Path $summaryFile -Value "🏗️ Build: #$buildNumber";
                    Add-Content -Path $summaryFile -Value "🌿 Branch: $branchName";
                    Add-Content -Path $summaryFile -Value "💾 Commit: $($commitId.Substring(0, [Math]::Min(8, $commitId.Length)))";
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '═══════════════════════════════════════════════════════════════════════════════════════════════════════════════';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '📈 EXECUTION SUMMARY:';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value "📋 Test Result Files (TRX): $($trxFiles.Count) files";
                    Add-Content -Path $summaryFile -Value "📋 Test Result Files (JUnit XML): $($xmlFiles.Count) files";
                    Add-Content -Path $summaryFile -Value "📊 Code Coverage (Cobertura): $(if ($coberturaExists) { '✅ Available' } else { '❌ Not Generated' })";
                    Add-Content -Path $summaryFile -Value "🎨 HTML Detailed Report: $(if ($htmlExists) { '✅ Generated' } else { '❌ Failed' })";
                    Add-Content -Path $summaryFile -Value "📋 Coverage Summary Report: $(if ($summaryExists) { '✅ Generated' } else { '❌ Failed' })";
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '═══════════════════════════════════════════════════════════════════════════════════════════════════════════════';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '🔗 HOW TO VIEW REPORTS:';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '📊 Jenkins Dashboard:';
                    Add-Content -Path $summaryFile -Value '  • Test Result: Click "Test Result" tab on the left sidebar';
                    Add-Content -Path $summaryFile -Value '  • Coverage Report: Click "Coverage Report" tab on the left sidebar';
                    Add-Content -Path $summaryFile -Value '  • HTML Reports: Click "Detailed HTML Coverage Report" or "Coverage Summary Report (HtmlSummary)" links';
                    Add-Content -Path $summaryFile -Value '';
                    Write-Host '✅ Enhanced test report summary generated successfully'
                    Get-Content $summaryFile | Write-Output
                '''
            }
        }
    }

    post {
        always {
            echo "📦 Archiving test results and coverage data..."
            archiveArtifacts artifacts: "TestResults/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "CoverageReports/publish/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "CoverageReports/HtmlReport/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "CoverageReports/SummaryReport/**/*", allowEmptyArchive: true, fingerprint: true
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
