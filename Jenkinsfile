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
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'
        DOTNET_NOLOGO = '1'
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
                        set -e
                        dotnet test "${TEST_PROJECT_PATH}" \\
                            --configuration "${BUILD_CONFIG}" \\
                            --no-build \\
                            --verbosity normal \\
                            --logger "trx;LogFileName=TestResults.trx" \\
                            --results-directory "${TEST_RESULTS_DIR}" \\
                            --collect:"XPlat Code Coverage"

                        # Đảm bảo có file JUnit (placeholder nếu không có test)
                        if ! ls "${TEST_RESULTS_DIR}"/*.xml >/dev/null 2>&1; then
                            cat > "${TEST_RESULTS_DIR}/junit.xml" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="CookBook Tests" tests="0" failures="0" errors="0" skipped="0" time="0">
  <properties />
</testsuite>
EOF
                        fi
                    '''
                }
            }
            post {
                always {
                    // Luôn publish JUnit (kể cả placeholder) để hiển thị kết quả
                    junit allowEmptyResults: true,
                          testResults: "${TEST_RESULTS_DIR}/*.xml",
                          skipPublishingChecks: false

                    // Publish TRX test results, cho phép rỗng để không fail
                    script {
                        try {
                            publishTestResults(
                                testResultsPattern: "${TEST_RESULTS_DIR}/**/*.trx",
                                testResultsFormat: 'MS TRX',
                                allowEmptyResults: true,
                                keepLongStdio: true,
                                healthScaleFactor: 1.0
                            )
                        } catch (err) {
                            echo "⚠️ Plugin publishTestResults không khả dụng hoặc gặp lỗi: ${err}"
                        }
                    }
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
                    // Publish code coverage reports (không fail khi không có report)
                    script {
                        try {
                            publishCoverage(
                                adapters: [
                                    coberturaAdapter(path: "${COVERAGE_DIR}/coverage.cobertura.xml")
                                ],
                                sourceFileResolver: sourceFiles('STORE_LAST_BUILD'),
                                calculateDiffForChangeRequests: true,
                                failNoReports: false,
                                globalThresholds: [[thresholdTarget: 'Line', unhealthyThreshold: '50', unstableThreshold: '0'],
                                                   [thresholdTarget: 'Branch', unhealthyThreshold: '50', unstableThreshold: '0']],
                                failUnhealthy: false,
                                failUnstable: false
                            )
                        } catch (err) {
                            echo "⚠️ Plugin Code Coverage API/publishCoverage không khả dụng hoặc gặp lỗi: ${err}"
                        }
                    }
                }
            }
        }
        
        stage('Test Report Summary') {
            steps {
                script {
                    echo '📋 Đang tạo Test Report Summary...'
                    sh '''
                        # Tạo test summary
                        TOTAL_TRX=$(ls -1 ${TEST_RESULTS_DIR}/*.trx 2>/dev/null | wc -l | sed 's/ //g')
                        HAS_COVERAGE="No"
                        if [ -f "${COVERAGE_DIR}/coverage.cobertura.xml" ]; then HAS_COVERAGE="Yes"; fi
                        
                        cat > "${TEST_RESULTS_DIR}/test-summary.txt" << EOF
╔══════════════════════════════════════════════════════════════╗
║         📊 BÁO CÁO KẾT QUẢ TEST CASE - COOKBOOK             ║
╚══════════════════════════════════════════════════════════════╝

Ngày chạy: $(date '+%d/%m/%Y %H:%M:%S')
Build Number: ${BUILD_NUMBER}
Branch: ${GIT_BRANCH}
Commit: ${GIT_COMMIT}

───────────────────────────────────────────────────────────────

📈 TỔNG QUAN KẾT QUẢ:

Số file TRX: ${TOTAL_TRX}
Lưu ý: Nếu không có Test Case, vẫn coi là PASSED (0 test).

───────────────────────────────────────────────────────────────

📁 CÁC FILE BÁO CÁO:
- Test Results (TRX): TestResults/*.trx
- Test Results (JUnit): TestResults/*.xml
- Code Coverage: CoverageReports/coverage.cobertura.xml (Available: ${HAS_COVERAGE})

───────────────────────────────────────────────────────────────

🔗 XEM CHI TIẾT:
- Test Results: Xem tab "Test Result" bên trái
- Code Coverage: Xem tab "Coverage Report" (nếu có) bên trái
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

