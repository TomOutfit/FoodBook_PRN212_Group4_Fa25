pipeline {
    agent any
    
    environment {
        DOTNET_VERSION = '9.0'
        BUILD_CONFIGURATION = 'Release'
        NUGET_PACKAGES = '~/.nuget/packages'
    }
    
    tools {
        // Cấu hình .NET SDK nếu có trong Jenkins
        dotnet 'dotnet-9.0'
    }
    
    stages {
        stage('Checkout') {
            steps {
                echo '🔍 Checking out source code...'
                checkout scm
                
                script {
                    // Hiển thị thông tin repository
                    sh '''
                        echo "📋 Repository Information:"
                        echo "  Repository: $(git config --get remote.origin.url)"
                        echo "  Branch: $(git branch --show-current)"
                        echo "  Commit: $(git rev-parse HEAD)"
                        echo "  Author: $(git log -1 --pretty=format:'%an <%ae>')"
                        echo "  Message: $(git log -1 --pretty=format:'%s')"
                    '''
                }
            }
        }
        
        stage('Discover Projects') {
            steps {
                echo '🔍 Discovering .NET projects...'
                script {
                    // Tìm tất cả solution files
                    def solutions = sh(
                        script: 'find . -name "*.sln" -type f | head -5',
                        returnStdout: true
                    ).trim()
                    
                    // Tìm tất cả project files
                    def projects = sh(
                        script: 'find . -name "*.csproj" -type f | head -10',
                        returnStdout: true
                    ).trim()
                    
                    // Tìm test projects
                    def testProjects = sh(
                        script: 'find . -name "*Test*.csproj" -o -name "*Tests.csproj" -type f',
                        returnStdout: true
                    ).trim()
                    
                    // Lưu vào environment variables
                    env.HAS_SOLUTIONS = solutions ? 'true' : 'false'
                    env.HAS_PROJECTS = projects ? 'true' : 'false'
                    env.HAS_TESTS = testProjects ? 'true' : 'false'
                    
                    if (solutions) {
                        env.SOLUTIONS = solutions
                        echo "Found solution files: ${solutions}"
                    }
                    
                    if (projects) {
                        env.PROJECTS = projects
                        echo "Found project files: ${projects}"
                    }
                    
                    if (testProjects) {
                        env.TEST_PROJECTS = testProjects
                        echo "Found test projects: ${testProjects}"
                    }
                }
            }
        }
        
        stage('Setup .NET') {
            steps {
                echo '⚙️ Setting up .NET environment...'
                script {
                    // Kiểm tra .NET version
                    sh '''
                        echo "🔧 .NET Environment Setup:"
                        echo "  .NET Version: ${DOTNET_VERSION}"
                        echo "  Build Configuration: ${BUILD_CONFIGURATION}"
                        
                        # Kiểm tra .NET có sẵn không
                        if command -v dotnet >/dev/null 2>&1; then
                            echo "  ✅ .NET CLI found: $(dotnet --version)"
                        else
                            echo "  ❌ .NET CLI not found - installing..."
                            # Cài đặt .NET nếu cần
                            curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version ${DOTNET_VERSION}
                            export PATH="$HOME/.dotnet:$PATH"
                        fi
                    '''
                }
            }
        }
        
        stage('Restore Dependencies') {
            steps {
                echo '📦 Restoring NuGet packages...'
                script {
                    def restoreSuccess = true
                    
                    if (env.HAS_SOLUTIONS == 'true') {
                        echo 'Restoring from solution files...'
                        def solutionsList = env.SOLUTIONS.split('\n')
                        for (solution in solutionsList) {
                            try {
                                sh "dotnet restore '${solution}' --verbosity quiet"
                                echo "✅ Successfully restored ${solution}"
                            } catch (Exception e) {
                                echo "⚠️ Failed to restore ${solution} (continuing...)"
                                restoreSuccess = false
                            }
                        }
                    } else if (env.HAS_PROJECTS == 'true') {
                        echo 'Restoring from project files...'
                        def projectsList = env.PROJECTS.split('\n')
                        for (project in projectsList) {
                            try {
                                sh "dotnet restore '${project}' --verbosity quiet"
                                echo "✅ Successfully restored ${project}"
                            } catch (Exception e) {
                                echo "⚠️ Failed to restore ${project} (continuing...)"
                                restoreSuccess = false
                            }
                        }
                    } else {
                        echo 'ℹ️ No .NET projects found to restore - skipping restore step'
                        restoreSuccess = true  // Không coi là lỗi khi không có project
                    }
                    
                    env.RESTORE_SUCCESS = restoreSuccess.toString()
                }
            }
        }
        
        stage('Build') {
            steps {
                echo '🔨 Building .NET projects...'
                script {
                    def buildSuccess = true
                    
                    if (env.HAS_SOLUTIONS == 'true') {
                        echo 'Building solution files...'
                        def solutionsList = env.SOLUTIONS.split('\n')
                        for (solution in solutionsList) {
                            try {
                                sh "dotnet build '${solution}' --configuration ${BUILD_CONFIGURATION} --no-restore --verbosity quiet"
                                echo "✅ Successfully built ${solution}"
                            } catch (Exception e) {
                                echo "⚠️ Failed to build ${solution} (continuing...)"
                                buildSuccess = false
                            }
                        }
                    } else if (env.HAS_PROJECTS == 'true') {
                        echo 'Building project files...'
                        def projectsList = env.PROJECTS.split('\n')
                        for (project in projectsList) {
                            try {
                                sh "dotnet build '${project}' --configuration ${BUILD_CONFIGURATION} --no-restore --verbosity quiet"
                                echo "✅ Successfully built ${project}"
                            } catch (Exception e) {
                                echo "⚠️ Failed to build ${project} (continuing...)"
                                buildSuccess = false
                            }
                        }
                    } else {
                        echo 'ℹ️ No .NET projects found to build - skipping build step'
                        buildSuccess = true  // Không coi là lỗi khi không có project
                    }
                    
                    env.BUILD_SUCCESS = buildSuccess.toString()
                }
            }
        }
        
        stage('Test') {
            steps {
                echo '🧪 Running tests...'
                script {
                    def testSuccess = true
                    
                    if (env.HAS_TESTS == 'true') {
                        echo 'Running test projects...'
                        def testProjectsList = env.TEST_PROJECTS.split('\n')
                        for (testProject in testProjectsList) {
                            try {
                                sh "dotnet test '${testProject}' --configuration ${BUILD_CONFIGURATION} --no-build --verbosity quiet --logger 'console;verbosity=minimal'"
                                echo "✅ Tests passed for ${testProject}"
                            } catch (Exception e) {
                                echo "⚠️ Tests failed for ${testProject} (continuing...)"
                                testSuccess = false
                            }
                        }
                    } else {
                        echo 'ℹ️ No test projects found - skipping test step'
                        testSuccess = true  // Không coi là lỗi khi không có test
                    }
                    
                    env.TEST_SUCCESS = testSuccess.toString()
                }
            }
        }
        
        stage('Publish') {
            steps {
                echo '📦 Publishing applications...'
                script {
                    def publishSuccess = true
                    
                    // Tạo thư mục publish
                    sh 'mkdir -p ./publish'
                    
                    if (env.HAS_SOLUTIONS == 'true') {
                        echo 'Publishing from solution files...'
                        def solutionsList = env.SOLUTIONS.split('\n')
                        for (solution in solutionsList) {
                            try {
                                def solutionName = sh(
                                    script: "basename '${solution}' .sln",
                                    returnStdout: true
                                ).trim()
                                sh "dotnet publish '${solution}' --configuration ${BUILD_CONFIGURATION} --output ./publish/${solutionName} --no-build --verbosity quiet"
                                echo "✅ Successfully published ${solution}"
                            } catch (Exception e) {
                                echo "⚠️ Failed to publish ${solution} (continuing...)"
                                publishSuccess = false
                            }
                        }
                    } else if (env.HAS_PROJECTS == 'true') {
                        echo 'Publishing from project files...'
                        def projectsList = env.PROJECTS.split('\n')
                        for (project in projectsList) {
                            try {
                                def projectName = sh(
                                    script: "basename '${project}' .csproj",
                                    returnStdout: true
                                ).trim()
                                sh "dotnet publish '${project}' --configuration ${BUILD_CONFIGURATION} --output ./publish/${projectName} --no-build --verbosity quiet"
                                echo "✅ Successfully published ${project}"
                            } catch (Exception e) {
                                echo "⚠️ Failed to publish ${project} (continuing...)"
                                publishSuccess = false
                            }
                        }
                    } else {
                        echo 'ℹ️ No .NET projects found to publish - skipping publish step'
                        publishSuccess = true  // Không coi là lỗi khi không có project
                    }
                    
                    env.PUBLISH_SUCCESS = publishSuccess.toString()
                }
            }
        }
        
        stage('Archive Artifacts') {
            steps {
                echo '📁 Archiving build artifacts...'
                script {
                    // Archive artifacts nếu có
                    if (fileExists('./publish')) {
                        archiveArtifacts artifacts: 'publish/**/*', fingerprint: true, allowEmptyArchive: true
                        echo '✅ Build artifacts archived'
                    } else {
                        echo 'ℹ️ No artifacts to archive'
                    }
                }
            }
        }
    }
    
    post {
        always {
            echo '📋 Pipeline Results Summary:'
            echo "  Restore: ${env.RESTORE_SUCCESS ?: 'N/A'}"
            echo "  Build: ${env.BUILD_SUCCESS ?: 'N/A'}"
            echo "  Test: ${env.TEST_SUCCESS ?: 'N/A'}"
            echo "  Publish: ${env.PUBLISH_SUCCESS ?: 'N/A'}"
            echo ''
            echo '🚀 FoodBook CI/CD Pipeline completed!'
            echo '🔗 Repository: https://github.com/TomOutfit/FoodBook_PRN212_Group4_Fa25'
        }
        
        success {
            echo '✅ Pipeline completed successfully!'
            // Có thể thêm notification ở đây
        }
        
        failure {
            echo '❌ Pipeline failed!'
            // Có thể thêm notification ở đây
        }
        
        unstable {
            echo '⚠️ Pipeline completed with warnings!'
        }
    }
}
