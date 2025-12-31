using System;
using System.Windows;
using System.Windows.Input;
using PL.Courier;

namespace PL;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// Modern login window for user authentication
/// Initializes database on startup before login
/// </summary>
public partial class LoginWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public LoginWindow()
    {
        InitializeComponent();
        InitializeDatabase();
    }

    /// <summary>
    /// Initializes the database with default data on startup.
    /// This ensures admin credentials are available for login.
    /// </summary>
    private void InitializeDatabase()
    {
        try
        {
            // Initialize database with test data
            s_bl.Admin.InitializeDB();
        }
        catch (Exception ex)
        {
            // Log error but don't prevent login window from showing
            // User can still try to login with existing data
            System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles the Login button click event.
    /// Validates the ID and authenticates the user.
    /// </summary>
    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        AttemptLogin();
    }

    /// <summary>
    /// Handles the Enter key press in the ID TextBox.
    /// Allows users to login by pressing Enter instead of clicking the button.
    /// </summary>
    private void IdTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
        {
            AttemptLogin();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Attempts to authenticate the user based on the entered ID.
    /// Validates the ID and checks user type (Admin or Courier).
    /// Opens appropriate window based on user type.
    /// </summary>
    private void AttemptLogin()
    {
        try
        {
            // Clear previous error message
            ErrorMessage.Text = string.Empty;

            // Validate ID is not empty
            if (string.IsNullOrWhiteSpace(IdTextBox.Text))
            {
                ErrorMessage.Text = "Please enter a valid ID";
                return;
            }

            // Validate user exists in the system
            // This will throw an exception if user doesn't exist
            var userType = s_bl.Courier.Login(IdTextBox.Text);

            if(userType == "Admin")
                new MainWindow().Show();
            else if(userType == "Courier")
                new CourierListWindow().Show();

            // Close login window
            this.Close();
        }
        catch (Exception ex)
        {
            // Display error message to user (user not found or invalid ID)
            ErrorMessage.Text = ex.Message;
        }
    }
}
