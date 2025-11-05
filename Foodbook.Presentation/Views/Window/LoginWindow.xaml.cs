using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Foodbook.Business.Interfaces;
using Foodbook.Business.Models;

namespace Foodbook.Presentation.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthenticationService _authService;
        private LoginModel _loginModel;
        
        public event EventHandler? LoginSuccessful;

        public LoginWindow(IAuthenticationService authService)
        {
            InitializeComponent();
            _authService = authService;
            _loginModel = new LoginModel();
            DataContext = _loginModel;
            
            // Initialize empty login model
            _loginModel.Email = "";
            _loginModel.Password = "";
            
            // Set focus to email field
            Loaded += (s, e) => this.EmailTextBox.Focus();
        }

        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update model from UI
                _loginModel.Email = this.EmailTextBox.Text;
                _loginModel.Password = this.PasswordBox.Password;

                // Validate input
                if (string.IsNullOrWhiteSpace(_loginModel.Email))
                {
                    MessageBox.Show("Please enter your email address.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.EmailTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(_loginModel.Password))
                {
                    MessageBox.Show("Please enter your password.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    this.PasswordBox.Focus();
                    return;
                }

                // Show loading state
                var signInButton = sender as Button;
                if (signInButton != null)
                {
                    var originalContent = signInButton.Content;
                    signInButton.Content = "Signing In...";
                    signInButton.IsEnabled = false;
                }

                // Attempt login
                var user = await _authService.LoginAsync(_loginModel);

                if (user != null)
                {
                    MessageBox.Show($"Welcome back, {user.Username}!", "Login Successful", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Set the result and close
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                    Close();
                }
                else
                {
                    MessageBox.Show("Invalid email or password. Please try again.", "Login Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during login: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restore button state
                if (sender is Button button)
                {
                    button.Content = "Sign In";
                    button.IsEnabled = true;
                }
            }
        }

        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Hide login window
                this.Hide();
                
                var registerWindow = new RegisterWindow(_authService);
                
                // Handle registration successful
                registerWindow.Closed += (s, args) =>
                {
                    // If registration was successful, trigger login success
                    if (registerWindow.DialogResult == true)
                    {
                        // Open MainWindow if it exists
                        if (Application.Current?.MainWindow != null)
                        {
                            var mainWindow = Application.Current.MainWindow;
                            mainWindow.WindowState = WindowState.Normal;
                            mainWindow.Topmost = true;
                            mainWindow.Show();
                            mainWindow.Activate();
                            mainWindow.Topmost = false;
                            mainWindow.Focus();
                        }
                        
                        LoginSuccessful?.Invoke(this, EventArgs.Empty);
                        this.Close(); // Close login window permanently
                    }
                    else
                    {
                        // Show login window again if user cancelled or closed
                        this.Show();
                        this.Activate();
                    }
                };
                
                // Show register window
                registerWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening registration window: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                // Show login window again on error
                this.Show();
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Password reset functionality will be implemented in a future update.", 
                "Forgot Password", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Handle Enter key for login
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
                SignIn_Click(sender, new RoutedEventArgs());
            }
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _loginModel.Email = EmailTextBox.Text;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _loginModel.Password = PasswordBox.Password;
        }

        private async void QuickAdminLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show loading state
                var button = sender as Button;
                if (button != null)
                {
                    var originalContent = button.Content;
                    button.Content = "Loading admin...";
                    button.IsEnabled = false;
                }

                // Get admin user from database
                var adminUser = await _authService.GetAdminUserAsync();

                if (adminUser == null)
                {
                    MessageBox.Show("No admin user found in the database. Please ensure the database is properly initialized.", 
                        "Admin Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Set admin credentials from database
                _loginModel.Email = adminUser.Email;
                _loginModel.Password = adminUser.Password; // Use stored plain text password for quick login
                
                // Update UI
                EmailTextBox.Text = _loginModel.Email;
                PasswordBox.Password = _loginModel.Password;

                // Update button text
                if (button != null)
                {
                    button.Content = "Logging in...";
                }

                // Attempt login
                var user = await _authService.LoginAsync(_loginModel);

                if (user != null)
                {
                    MessageBox.Show($"Welcome back, {user.Username}!", "Quick Admin Login Successful", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Set the result and close
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                    Close();
                }
                else
                {
                    MessageBox.Show("Admin login failed. Please check database connection or credentials.", "Login Failed", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during admin login: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restore button state
                if (sender is Button btn)
                {
                    btn.Content = "🚀 Quick Login as Admin";
                    btn.IsEnabled = true;
                }
            }
        }
    }
}
