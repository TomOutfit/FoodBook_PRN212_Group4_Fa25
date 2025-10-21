using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Foodbook.Business.Interfaces;
using Foodbook.Business.Models;

namespace Foodbook.Presentation.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly IAuthenticationService _authService;
        private RegisterModel _registerModel;

        public RegisterWindow(IAuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
            _registerModel = new RegisterModel();
            
            // Set focus to username field
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private async void SignUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update model from UI
                _registerModel.Username = UsernameTextBox.Text;
                _registerModel.Email = EmailTextBox.Text;
                _registerModel.Password = PasswordBox.Password;
                _registerModel.ConfirmPassword = ConfirmPasswordBox.Password;

                // Validate input
                if (string.IsNullOrWhiteSpace(_registerModel.Username))
                {
                    MessageBox.Show("Please enter a username.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    UsernameTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_registerModel.Email))
                {
                    MessageBox.Show("Please enter your email address.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    EmailTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_registerModel.Password))
                {
                    MessageBox.Show("Please enter a password.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PasswordBox.Focus();
                    return;
                }

                if (_registerModel.Password != _registerModel.ConfirmPassword)
                {
                    MessageBox.Show("Passwords do not match.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ConfirmPasswordBox.Focus();
                    return;
                }

                if (!_registerModel.AgreeToTerms)
                {
                    MessageBox.Show("Please agree to the Terms of Service and Privacy Policy.", 
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show loading state
                var signUpButton = sender as Button;
                var originalContent = signUpButton?.Content;
                signUpButton.Content = "Creating Account...";
                signUpButton.IsEnabled = false;

                // Attempt registration
                var user = await _authService.RegisterAsync(_registerModel);

                if (user != null)
                {
                    MessageBox.Show($"Welcome to Foodbook, {user.Username}! Your account has been created successfully.", 
                        "Registration Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Set the result and close
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Registration failed. The email or username may already be in use.", 
                        "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during registration: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restore button state
                if (sender is Button button)
                {
                    button.Content = "Create Account";
                    button.IsEnabled = true;
                }
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginWindow = new LoginWindow(_authService);
                loginWindow.Owner = this;
                
                if (loginWindow.ShowDialog() == true)
                {
                    // Login successful, close registration window
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening login window: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // Handle Enter key navigation
        private void UsernameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EmailTextBox.Focus();
            }
        }

        private void EmailTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordBox.Focus();
            }
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmPasswordBox.Focus();
            }
        }

        private void ConfirmPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SignUp_Click(sender, new RoutedEventArgs());
            }
        }
    }
}
