pipeline {
    agent any

    environment {
        SOLUTION_PATH = "CookBook.sln"
        BUILD_CONFIG = "Release"
        TEST_PROJECT = "FoodBook.Tests/FoodBook.Tests.csproj"
        TEST_RESULTS_DIR = "TestResults"
    }

    stages {
        stage('Clean') {
            steps {
                echo '🧹 Dọn dẹp workspace...'
                bat """
                    dotnet clean "%SOLUTION_PATH%" --configuration "%BUILD_CONFIG%"
                    rmdir /s /q "%TEST_RESULTS_DIR%" 2>nul
                    rmdir /s /q "%COVERAGE_DIR%" 2>nul
                    mkdir "%TEST_RESULTS_DIR%"
                    mkdir "%COVERAGE_DIR%"
                """
            }
        }


        stage('📦 Restore') {
            steps {
                bat """
                    dotnet restore "%SOLUTION_PATH%"
                """
            }
        }

        stage('🏗️ Build') {
            steps {
                bat """
                    dotnet build "%SOLUTION_PATH%" --configuration "%BUILD_CONFIG%" --no-restore
                """
            }
        }

        stage('🧪 Run Unit Tests') {
            steps {
                bat """
                    if not exist "%TEST_RESULTS_DIR%" mkdir "%TEST_RESULTS_DIR%"
                    dotnet test "%TEST_PROJECT%" --configuration "%BUILD_CONFIG%" --logger "trx;LogFileName=%TEST_RESULTS_DIR%\\junit.xml" --results-directory "%TEST_RESULTS_DIR%" --no-build
                """
            }
        }

        stage('📊 Publish Test Report') {
            steps {
                junit allowEmptyResults: true, testResults: "%TEST_RESULTS_DIR%\\junit.xml"
            }
        }
    }

    post {
        success {
            echo "✅ Build & Tests successful!"
        }
        failure {
            echo "❌ Build failed. Check logs."
        }
    }
}

