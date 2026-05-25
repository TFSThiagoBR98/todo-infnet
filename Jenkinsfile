pipeline {
    agent any

    environment {
        DOCKERHUB_CREDENTIALS = credentials('dockerhub-credentials')
        IMAGE_NAME             = 'tfsthiagobr98/todo-infnet'
        K8S_DIR                = 'k8s'
        KUBECONFIG             = '/var/jenkins_home/.kube/config'
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
                echo "Codigo obtido: branch ${env.BRANCH_NAME ?: 'main'}, build #${BUILD_NUMBER}"
            }
        }

        stage('Build da Imagem Docker') {
            steps {
                sh """
                    docker build \\
                        -t ${IMAGE_NAME}:latest \\
                        -t ${IMAGE_NAME}:build-${BUILD_NUMBER} \\
                        .
                """
            }
        }

        stage('Push para Docker Hub') {
            steps {
                sh 'echo $DOCKERHUB_CREDENTIALS_PSW | docker login -u $DOCKERHUB_CREDENTIALS_USR --password-stdin'
                sh "docker push ${IMAGE_NAME}:latest"
                sh "docker push ${IMAGE_NAME}:build-${BUILD_NUMBER}"
            }
        }

        stage('Rolling Update no Kubernetes') {
            steps {
                sh """
                    kubectl --kubeconfig=/var/jenkins_home/.kube/config \
                        set image deployment/todo-infnet \
                        todo-infnet=${IMAGE_NAME}:build-${BUILD_NUMBER}
                """
                sh "kubectl --kubeconfig=/var/jenkins_home/.kube/config rollout status deployment/todo-infnet --timeout=300s"
            }
        }

        stage('Verificar Deploy') {
            steps {
                sh "kubectl --kubeconfig=/var/jenkins_home/.kube/config get pods -o wide"
                sh "kubectl --kubeconfig=/var/jenkins_home/.kube/config get svc todo-app-service"
            }
        }

    }

    post {
        success {
            echo "Deploy concluido! Imagem: ${IMAGE_NAME}:build-${BUILD_NUMBER}"
        }
        failure {
            echo "Pipeline falhou no build #${BUILD_NUMBER}. Verificar logs acima."
        }
        always {
            sh 'docker logout || true'
        }
    }
}
