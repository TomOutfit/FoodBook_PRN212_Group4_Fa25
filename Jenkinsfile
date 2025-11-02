pipeline {
    agent any
    
    options {
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
    }
    
    tools {
        dotnetSDK 'dotnet-sdk-9.0'
    }
    
    stages {
        stage('Checkout') {
            steps {
                script {
                    echo '📥 Đang checkout source code...'
                    checkout scm
                }
            }
        }
        
        stage('Clean') {
            steps {
                script {
                    echo '🧹 Đang dọn dẹp workspace...'
                    sh '''
                        dotnet clean "${SOLUTION_PATH}" --configuration "${BUILD_CONFIG}" --verbosity minimal
                        rm -rf "${TEST_RESULTS_DIR}" || true
                        rm -rf "${COVERAGE_DIR}" || true
                        mkdir -p "${TEST_RESULTS_DIR}"
                        mkdir -p "${COVERAGE_DIR}"
                    '''
                }
            }
        }
        
        stage('Restore') {
            steps {
                script {
                    echo '📦 Đang restore NuGet packages...'
                    sh 'dotnet restore "${SOLUTION_PATH}" --verbosity minimal'
                }
            }
        }
        
        stage('Build') {
            steps {
                script {
                    echo '🔨 Đang build solution...'
                    sh 'dotnet build "${SOLUTION_PATH}" --configuration "${BUILD_CONFIG}" --no-restore --verbosity minimal'
                }
            }
        }
        
        stage('Unit Tests') {
            steps {
                script {
                    echo '🧪 Đang chạy Unit Tests...'
                    sh '''
                        dotnet test "${TEST_PROJECT_PATH}" \\
                            --configuration "${BUILD_CONFIG}" \\
                            --no-build \\
                            --verbosity normal \\
                            --logger "trx;LogFileName=TestResults.trx" \\
                            --logger "junit;LogFilePath=${TEST_RESULTS_DIR}/junit.xml" \\
                            --results-directory "${TEST_RESULTS_DIR}" \\
                            --collect:"XPlat Code Coverage" \\
                            --settings:/RunSettings/DataCollectionRunSettings/DataCollectors/DataCollector/Configuration/Format[0]=cobertura \\
                            --settings:/RunSettings/DataCollectionRunSettings/DataCollectors/DataCollector/Configuration/Format[1]=json \\
                            --settings:/RunSettings/DataCollectionRunSettings/DataCollectors/DataCollector/Configuration/Format[2]=opencover \\
                            -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.CoverageFilePath="${COVERAGE_DIR}/coverage.cobertura.xml"
                    '''
                }
            }
            post {
                always {
                    // Publish JUnit test results
                    junit allowEmptyResults: true,
                          testResultsPattern: "${TEST_RESULTS_DIR}/junit.xml",
                          testResultsFormat: 'JUnit',
                          keepLongStdio: true,
                          healthScaleFactor: 1.0
                    
                    // Publish TRX test results
                    publishTestResults(
                        testResultsPattern: "${TEST_RESULTS_DIR}/**/*.trx",
                        testResultsFormat: 'MS TRX',
                        allowEmptyResults: true,
                        keepLongStdio: true,
                        healthScaleFactor: 1.0
                    )
                }
            }
        }
        
        stage('Code Coverage') {
            steps {
                script {
                    echo '📊 Đang xử lý Code Coverage...'
                    sh '''
                        # Tìm coverage file
                        COVERAGE_FILE=$(find "${TEST_RESULTS_DIR}" -name "coverage.cobertura.xml" | head -1)
                        
                        if [ -f "$COVERAGE_FILE" ]; then
                            echo "✅ Tìm thấy coverage file: $COVERAGE_FILE"
                            cp "$COVERAGE_FILE" "${COVERAGE_DIR}/coverage.cobertura.xml"
                        else
                            echo "⚠️ Không tìm thấy coverage file"
                        fi
                    '''
                }
            }
            post {
                always {
                    // Publish code coverage reports
                    publishCoverageReport(
                        adapters: [
                            coberturaAdapter("${COVERAGE_DIR}/coverage.cobertura.xml")
                        ],
                        sourceFileResolver: sourceFiles('STORE_LAST_BUILD'),
                        calculateDiffForChangeRequests: true,
                        conditionalCoverage: true,
                        failUnhealthy: false,
                        failUnstable: false,
                        healthy: 80,
                        unhealthy: 50,
                        autoUpdateHealth: true,
                        autoUpdateStability: true,
                        coberturaReportFile: "${COVERAGE_DIR}/coverage.cobertura.xml"
                    )
                }
            }
        }
        
        stage('Test Report Summary') {
            steps {
                script {
                    echo '📋 Đang tạo Test Report Summary...'
                    sh '''
                        # Tạo test summary
                        cat > "${TEST_RESULTS_DIR}/test-summary.txt" << 'EOF'
╔══════════════════════════════════════════════════════════════╗
║         📊 BÁO CÁO KẾT QUẢ TEST CASE - COOKBOOK             ║
╚══════════════════════════════════════════════════════════════╝

Ngày chạy: $(date '+%d/%m/%Y %H:%M:%S')
Build Number: ${BUILD_NUMBER}
Branch: ${GIT_BRANCH}
Commit: ${GIT_COMMIT}

───────────────────────────────────────────────────────────────

📈 TỔNG QUAN KẾT QUẢ:

Xem chi tiết trong Test Results và Coverage Report bên dưới.

───────────────────────────────────────────────────────────────

📁 CÁC FILE BÁO CÁO:
- Test Results (TRX): TestResults/*.trx
- Test Results (JUnit): TestResults/junit.xml
- Code Coverage: CoverageReports/coverage.cobertura.xml

───────────────────────────────────────────────────────────────

🔗 XEM CHI TIẾT:
- Test Results: Xem tab "Test Result" bên trái
- Code Coverage: Xem tab "Coverage Report" bên trái
- Console Output: Xem "Console Output" để xem log chi tiết

EOF
                        cat "${TEST_RESULTS_DIR}/test-summary.txt"
                    '''
                }
            }
        }
    }
    
    post {
        always {
            script {
                echo '📦 Đang archive test artifacts...'
                archiveArtifacts artifacts: "${TEST_RESULTS_DIR}/**/*", 
                                allowEmptyArchive: true,
                                fingerprint: true
                archiveArtifacts artifacts: "${COVERAGE_DIR}/**/*",
                                allowEmptyArchive: true,
                                fingerprint: true
            }
        }
        
        success {
            script {
                echo '✅ ✅ ✅ BUILD THÀNH CÔNG! ✅ ✅ ✅'
                echo 'Tất cả tests đã pass. Kiểm tra Test Results và Coverage Report để xem chi tiết.'
            }
        }
        
        failure {
            script {
                echo '❌ ❌ ❌ BUILD THẤT BẠI! ❌ ❌ ❌'
                echo 'Có một số tests fail hoặc build bị lỗi. Vui lòng kiểm tra log để xem chi tiết.'
            }
        }
        
        unstable {
            script {
                echo '⚠️ ⚠️ ⚠️ BUILD KHÔNG ỔN ĐỊNH! ⚠️ ⚠️ ⚠️'
                echo 'Build thành công nhưng có một số warnings hoặc tests bị skip.'
            }
        }
    }
}
