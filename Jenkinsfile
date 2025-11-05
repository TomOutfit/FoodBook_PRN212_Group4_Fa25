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

    stages {
        stage('Checkout') {
            steps {
                echo '📥 Đang checkout source code...'
                checkout scm
            }
        }

        stage('Clean') {
            steps {
                echo '🧹 Dọn dẹp workspace...'
                sh '''
                    dotnet clean "$SOLUTION_PATH" --configuration "$BUILD_CONFIG" --verbosity minimal
                    rm -rf "$TEST_RESULTS_DIR" || true
                    rm -rf "$COVERAGE_DIR" || true
                    mkdir -p "$TEST_RESULTS_DIR"
                    mkdir -p "$COVERAGE_DIR"
                '''
            }
        }

        stage('Restore') {
            steps {
                echo '📦 Đang restore NuGet packages...'
                sh "dotnet restore \"$SOLUTION_PATH\" --verbosity minimal"
            }
        }

        stage('Build') {
            steps {
                echo '🔨 Đang build solution...'
                sh "dotnet build \"$SOLUTION_PATH\" --configuration \"$BUILD_CONFIG\" --no-restore --verbosity minimal"
            }
        }

        stage('Unit Tests') {
            steps {
                echo '🧪 Đang chạy Unit Tests...'
                script {
                    def testDir = env.TEST_RESULTS_DIR
                    def coverDir = env.COVERAGE_DIR
                    def testProj = env.TEST_PROJECT_PATH

                    if (fileExists(testProj)) {
                        sh """
                            dotnet test "$testProj" \
                                --configuration "$BUILD_CONFIG" \
                                --no-build \
                                --verbosity normal \
                                --logger "trx;LogFileName=TestResults.trx" \
                                --logger "junit;LogFilePath=$testDir/junit.xml" \
                                --results-directory "$testDir" \
                                --collect:"XPlat Code Coverage"
                        """
                    } else {
                        echo "⚠️ Không có TestCase (Không tìm thấy project test: $testProj)"
                    }
                }
            }
            post {
                always {
                    junit allowEmptyResults: true,
                          testResultsPattern: "${TEST_RESULTS_DIR}/junit.xml"

                    publishTestResults(
                        testResultsPattern: "${TEST_RESULTS_DIR}/**/*.trx",
                        testResultsFormat: 'MS TRX',
                        allowEmptyResults: true
                    )
                }
            }
        }

        stage('Code Coverage') {
            steps {
                echo '📊 Đang xử lý Code Coverage...'
                sh '''
                    COVERAGE_FILE=$(find "$TEST_RESULTS_DIR" -name "coverage.cobertura.xml" | head -1)
                    if [ -f "$COVERAGE_FILE" ]; then
                        echo "✅ Tìm thấy coverage file: $COVERAGE_FILE"
                        cp "$COVERAGE_FILE" "$COVERAGE_DIR/coverage.cobertura.xml"
                    else
                        echo "⚠️ Không tìm thấy coverage file"
                    fi
                '''
            }
            post {
                always {
                    publishCoverage adapters: [
                        coberturaAdapter("${COVERAGE_DIR}/coverage.cobertura.xml")
                    ],
                    sourceFileResolver: sourceFiles('STORE_LAST_BUILD'),
                    failUnhealthy: false,
                    failUnstable: false
                }
            }
        }

        stage('Test Report Summary') {
            steps {
                echo '📋 Tạo Test Report Summary...'
                sh '''
                    cat > "$TEST_RESULTS_DIR/test-summary.txt" << 'EOF'
╔══════════════════════════════════════════════════════════════╗
║         📊 BÁO CÁO KẾT QUẢ TEST CASE - FOODBOOK              ║
╚══════════════════════════════════════════════════════════════╝

Ngày chạy: $(date '+%d/%m/%Y %H:%M:%S')
Build Number: ${BUILD_NUMBER}
Branch: ${GIT_BRANCH}
Commit: ${GIT_COMMIT}

───────────────────────────────────────────────────────────────
📈 TỔNG QUAN:
Xem chi tiết trong Test Results và Coverage Report bên dưới.

───────────────────────────────────────────────────────────────
📁 CÁC FILE BÁO CÁO:
- Test Results: TestResults/*.trx, junit.xml
- Code Coverage: CoverageReports/coverage.cobertura.xml

───────────────────────────────────────────────────────────────
EOF
                    cat "$TEST_RESULTS_DIR/test-summary.txt"
                '''
            }
        }
    }

    post {
        always {
            echo '📦 Lưu trữ artifacts...'
            archiveArtifacts artifacts: "${TEST_RESULTS_DIR}/**/*", allowEmptyArchive: true
            archiveArtifacts artifacts: "${COVERAGE_DIR}/**/*", allowEmptyArchive: true
        }

        success {
            echo '✅ BUILD THÀNH CÔNG! Xem Test Results và Coverage Report.'
        }

        failure {
            echo '❌ BUILD THẤT BẠI! Kiểm tra console log.'
        }

        unstable {
            echo '⚠️ BUILD KHÔNG ỔN ĐỊNH (Cảnh báo hoặc test skip).'
        }
    }
}
