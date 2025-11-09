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
                // IMPROVED: Enhanced test execution with better error handling and coverage collection
                bat '''
                    dotnet test "%TEST_PROJECT_PATH%" ^
                        --configuration "%BUILD_CONFIG%" ^
                        --no-build ^
                        --logger "trx;LogFileName=TestResults.trx" ^
                        --results-directory "%TEST_RESULTS_DIR%" ^
                        --verbosity normal ^
                        /p:CollectCoverage=true ^
                        /p:CoverletOutput="%TEST_RESULTS_DIR%/" ^
                        /p:CoverletOutputFormat=cobertura ^
                        /p:CoverletOutputFormat=json ^
                        /p:CoverletOutputFormat=opencover ^
                        /p:ExcludeByAttribute="Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute" ^
                        /p:SkipAutoProps=true
                '''
            }
            post {
                always {
                    echo "📄 Processing and publishing test results..."
                    // IMPROVED: Convert TRX to JUnit XML for better Jenkins integration
                    powershell '''
                        $trxFiles = Get-ChildItem -Path 'TestResults' -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue
                        if ($trxFiles) {
                            Write-Host "✅ Found TRX files: $($trxFiles.Count)"
                            foreach ($file in $trxFiles) {
                                Write-Host "  - $($file.FullName)"
                                # Convert TRX to JUnit XML for better reporting
                                $xmlFileName = [System.IO.Path]::ChangeExtension($file.Name, '.xml')
                                $xmlFilePath = Join-Path 'TestResults' $xmlFileName
                                try {
                                    trx2junit "$($file.FullName)" "$xmlFilePath"
                                    Write-Host "    -> Converted to: $xmlFilePath"
                                } catch {
                                    Write-Host "    -> Conversion failed: $($_.Exception.Message)"
                                }
                            }
                        } else {
                            Write-Host "⚠️ No TRX files found in TestResults directory"
                        }
                    '''
                    // Publish both TRX and converted XML files
                    junit allowEmptyResults: true, testResults: "${TEST_RESULTS_DIR}/*.xml"

                    // Diagnostic log: Check for test results files
                    powershell '''
                        $trxFiles = Get-ChildItem -Path 'TestResults' -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue
                        if ($trxFiles) {
                            Write-Host "✅ Found TRX files: $($trxFiles.Count)"
                            foreach ($file in $trxFiles) { Write-Host "  - $($file.FullName)" }
                        } else {
                            Write-Host "⚠️ No TRX files found in TestResults directory"
                        }
                        $xmlFiles = Get-ChildItem -Path 'TestResults' -Filter '*.xml' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne 'coverage.cobertura.xml' }
                        if ($xmlFiles) {
                            Write-Host "✅ Found JUnit XML files: $($xmlFiles.Count)"
                            foreach ($file in $xmlFiles) { Write-Host "  - $($file.FullName)" }
                        } else {
                            Write-Host "⚠️ No JUnit XML files found"
                        }
                    '''
                }
            }
        }

        stage('Code Coverage') {
            steps {
                echo "📊 Processing and publishing coverage reports..."
                script {
                    // IMPROVED: Enhanced coverage file handling with multiple format support
                    powershell '''
                        # 1. Find and copy coverage files (support multiple formats)
                        $coberturaFile = Get-ChildItem -Recurse -Path 'TestResults' -Filter 'coverage.cobertura.xml' | Select-Object -First 1;
                        $opencoverFile = Get-ChildItem -Recurse -Path 'TestResults' -Filter 'coverage.opencover.xml' | Select-Object -First 1;

                        if ($coberturaFile) {
                            Write-Host '✅ Found Cobertura coverage file:' $coberturaFile.FullName;
                            Copy-Item $coberturaFile.FullName 'CoverageReports/coverage.cobertura.xml' -Force
                            Write-Host '✅ Copied Cobertura file to CoverageReports/'
                        } else {
                            Write-Host '⚠️ No Cobertura coverage file found'
                        }

                        if ($opencoverFile) {
                            Write-Host '✅ Found OpenCover coverage file:' $opencoverFile.FullName;
                            Copy-Item $opencoverFile.FullName 'CoverageReports/coverage.opencover.xml' -Force
                            Write-Host '✅ Copied OpenCover file to CoverageReports/'
                        } else {
                            Write-Host '⚠️ No OpenCover coverage file found'
                        }

                        # Diagnostic: Check copied files
                        $copiedCobertura = 'CoverageReports/coverage.cobertura.xml'
                        $copiedOpencover = 'CoverageReports/coverage.opencover.xml'

                        if (Test-Path $copiedCobertura) {
                            $fileSize = (Get-Item $copiedCobertura).Length
                            Write-Host "✅ Cobertura file ready: $fileSize bytes"
                        } else {
                            Write-Host '❌ Cobertura file not found after copy'
                        }

                        if (Test-Path $copiedOpencover) {
                            $fileSize = (Get-Item $copiedOpencover).Length
                            Write-Host "✅ OpenCover file ready: $fileSize bytes"
                        } else {
                            Write-Host '❌ OpenCover file not found after copy'
                        }
                    '''

                    def finalCoberturaFile = "${COVERAGE_DIR}/coverage.cobertura.xml"
                    def finalOpencoverFile = "${COVERAGE_DIR}/coverage.opencover.xml"

                    // Publish coverage reports with enhanced adapters
                    def adapters = []
                    if (fileExists(finalCoberturaFile)) {
                        adapters.add(coberturaAdapter(finalCoberturaFile))
                        echo "✅ Added Cobertura coverage adapter"
                    }
                    if (fileExists(finalOpencoverFile)) {
                        adapters.add(istanbulCoberturaAdapter(finalOpencoverFile))  // Note: Using istanbul for OpenCover support
                        echo "✅ Added OpenCover coverage adapter"
                    }

                    if (!adapters.isEmpty()) {
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
                    def finalCoberturaFile = "${COVERAGE_DIR}/coverage.cobertura.xml"
                    def finalOpencoverFile = "${COVERAGE_DIR}/coverage.opencover.xml"

                    // IMPROVED: Generate multiple report formats for beautiful visualization
                    def reportFiles = []
                    if (fileExists(finalCoberturaFile)) {
                        reportFiles.add(finalCoberturaFile)
                    }
                    if (fileExists(finalOpencoverFile)) {
                        reportFiles.add(finalOpencoverFile)
                    }

                    if (!reportFiles.isEmpty()) {
                        def reportsArg = reportFiles.join(';')

                        // Generate multiple beautiful report formats
                        bat """
                            reportgenerator ^
                                -reports:"${reportsArg}" ^
                                -targetdir:"%COVERAGE_DIR%/HtmlReport" ^
                                -reporttypes:Html;HtmlChart;HtmlSummary ^
                                -title:"CookBook Coverage Report" ^
                                -tag:"${BUILD_NUMBER}" ^
                                -verbosity:Info
                        """

                        // Generate additional summary report
                        bat """
                            reportgenerator ^
                                -reports:"${reportsArg}" ^
                                -targetdir:"%COVERAGE_DIR%/SummaryReport" ^
                                -reporttypes:HtmlInline_AzurePipelines ^
                                -title:"CookBook Test Summary" ^
                                -tag:"${BUILD_NUMBER}"
                        """

                        // Diagnostic: Check if HTML reports were generated successfully
                        powershell '''
                            $htmlDir = 'CoverageReports/HtmlReport'
                            $summaryDir = 'CoverageReports/SummaryReport'

                            if (Test-Path $htmlDir) {
                                $indexFile = Join-Path $htmlDir 'index.html'
                                if (Test-Path $indexFile) {
                                    Write-Host '✅ Main HTML report generated successfully'
                                    $fileSize = (Get-Item $indexFile).Length
                                    Write-Host "Index file size: $fileSize bytes"
                                    $totalFiles = (Get-ChildItem -Path $htmlDir -Recurse -File).Count
                                    Write-Host "Total files in HTML report: $totalFiles"
                                } else {
                                    Write-Host '❌ index.html not found in HtmlReport directory'
                                }
                            } else {
                                Write-Host '❌ HtmlReport directory not created'
                            }

                            if (Test-Path $summaryDir) {
                                $summaryFile = Join-Path $summaryDir 'index.html'
                                if (Test-Path $summaryFile) {
                                    Write-Host '✅ Summary report generated successfully'
                                    $fileSize = (Get-Item $summaryFile).Length
                                    Write-Host "Summary file size: $fileSize bytes"
                                } else {
                                    Write-Host '❌ Summary index.html not found'
                                }
                            } else {
                                Write-Host '❌ SummaryReport directory not created'
                            }
                        '''

                        // Publish multiple HTML reports to Jenkins
                        echo "📊 Publishing enhanced HTML reports to Jenkins..."

                        publishHTML(
                            target: [
                                allowMissing: false,
                                directory: "${COVERAGE_DIR}/HtmlReport",
                                indexPages: 'index.html',
                                keepAll: true,
                                reportName: 'Detailed HTML Coverage Report'
                            ]
                        )

                        publishHTML(
                            target: [
                                allowMissing: false,
                                directory: "${COVERAGE_DIR}/SummaryReport",
                                indexPages: 'index.html',
                                keepAll: true,
                                reportName: 'Coverage Summary Report'
                            ]
                        )

                        echo "✅ Enhanced HTML reports published to Jenkins"
                    } else {
                        echo "⚠️ Không tìm thấy file coverage nào, bỏ qua HTML report generation."
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
                echo '📋 Creating enhanced Test Report Summary...'
                // IMPROVED: Enhanced summary with more detailed information and beautiful formatting
                powershell '''
                    $summaryFile = 'TestResults/test-summary.txt';

                    $buildNumber = $env:BUILD_NUMBER;
                    $branchName = $env:GIT_BRANCH;
                    $commitId = $env:GIT_COMMIT;

                    # Enhanced file counting
                    $trxFiles = @(Get-ChildItem -Path 'TestResults' -Filter '*.trx' -Recurse -ErrorAction SilentlyContinue);
                    $xmlFiles = @(Get-ChildItem -Path 'TestResults' -Filter '*.xml' -Recurse -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne 'coverage.cobertura.xml' -and $_.Name -ne 'coverage.opencover.xml' });
                    $coberturaExists = Test-Path 'CoverageReports/coverage.cobertura.xml';
                    $opencoverExists = Test-Path 'CoverageReports/coverage.opencover.xml';
                    $htmlExists = Test-Path 'CoverageReports/HtmlReport/index.html';
                    $summaryExists = Test-Path 'CoverageReports/SummaryReport/index.html';

                    New-Item -ItemType Directory -Force -Path 'TestResults' | Out-Null;

                    # Beautiful ASCII art header
                    Set-Content -Path $summaryFile -Value '╔══════════════════════════════════════════════════════════════════════════════════════════════════════════════╗' -Encoding UTF8;
                    Add-Content -Path $summaryFile -Value '║                                           📊 COOKBOOK TEST REPORT                                           ║';
                    Add-Content -Path $summaryFile -Value '╚══════════════════════════════════════════════════════════════════════════════════════════════════════════════╝';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value "⏰ Execution Time: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')";
                    Add-Content -Path $summaryFile -Value "🏗️  Build: #$buildNumber";
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
                    Add-Content -Path $summaryFile -Value "📊 Code Coverage (OpenCover): $(if ($opencoverExists) { '✅ Available' } else { '❌ Not Generated' })";
                    Add-Content -Path $summaryFile -Value "🎨 HTML Detailed Report: $(if ($htmlExists) { '✅ Generated' } else { '❌ Failed' })";
                    Add-Content -Path $summaryFile -Value "📋 Coverage Summary Report: $(if ($summaryExists) { '✅ Generated' } else { '❌ Failed' })";
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '═══════════════════════════════════════════════════════════════════════════════════════════════════════════════';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '📁 GENERATED REPORTS:';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '🔹 Test Results:';
                    Add-Content -Path $summaryFile -Value '  • TRX Format: TestResults/*.trx';
                    Add-Content -Path $summaryFile -Value '  • JUnit XML: TestResults/*.xml (converted for better Jenkins integration)';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '🔹 Code Coverage:';
                    Add-Content -Path $summaryFile -Value '  • Raw Data: CoverageReports/coverage.*.xml';
                    Add-Content -Path $summaryFile -Value '  • Jenkins UI: Integrated coverage graphs and metrics';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '🔹 HTML Reports:';
                    Add-Content -Path $summaryFile -Value '  • Detailed: CoverageReports/HtmlReport/index.html';
                    Add-Content -Path $summaryFile -Value '  • Summary: CoverageReports/SummaryReport/index.html';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '═══════════════════════════════════════════════════════════════════════════════════════════════════════════════';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '🔗 HOW TO VIEW REPORTS:';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '📊 Jenkins Dashboard:';
                    Add-Content -Path $summaryFile -Value '  • Test Result: Click "Test Result" tab on the left sidebar';
                    Add-Content -Path $summaryFile -Value '  • Coverage Report: Click "Coverage Report" tab on the left sidebar';
                    Add-Content -Path $summaryFile -Value '  • HTML Reports: Click "Detailed HTML Coverage Report" or "Coverage Summary Report" links';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '📋 Download Options:';
                    Add-Content -Path $summaryFile -Value '  • All artifacts are archived and available for download';
                    Add-Content -Path $summaryFile -Value '  • Raw data files can be downloaded for external analysis';
                    Add-Content -Path $summaryFile -Value '';
                    Add-Content -Path $summaryFile -Value '⚠️  Note: If no tests are executed, status is still PASSED (0 tests = success)';
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
            archiveArtifacts artifacts: "${TEST_RESULTS_DIR}/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "${COVERAGE_DIR}/publish/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "${COVERAGE_DIR}/HtmlReport/**/*", allowEmptyArchive: true, fingerprint: true
            archiveArtifacts artifacts: "${COVERAGE_DIR}/SummaryReport/**/*", allowEmptyArchive: true, fingerprint: true
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
