using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Security;
using System.Text.RegularExpressions;

namespace proiect_pass_storage {
    
    public partial class AuthorizationPage : Window {
        private const string DIRECTORY_PATH = "D:\\pass_app_directory";
        private const string PASSPHRASE = "You never kill the sun!";
        private List<string> usersNames;

        public AuthorizationPage()
        {
            InitializeComponent();
            usersNames = new List<string>();
            var files = checkStorageForFiles();
            if (files.Count() == 0) {
                passInfoLabel.Content = "Create new user";
                checkBoxCreateNewAcc.Visibility = Visibility.Collapsed;// or:System.Windows.Visibility.Hidden
                checkBoxCreateNewAcc.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                selectUser.Visibility = Visibility.Collapsed;
                usernameBox.Visibility = Visibility.Visible;

            }
            else {
                setUsersNames(files);
                checkBoxCreateNewAcc.Checked += new RoutedEventHandler(AddNewUserAction);
                checkBoxCreateNewAcc.Unchecked += new RoutedEventHandler(AddNewUserAction);
            }
            buttonApply.Click += new RoutedEventHandler(AuthorizeUser);
        }

        /// <summary>
        /// Button click handler. Authorise user or create new.
        /// </summary>
        /// <param name="sender">Object item</param>
        /// <param name="e">Event</param>
        private void AuthorizeUser(object sender, RoutedEventArgs e)
        {
            buttonApply = sender as Button;
            var password = passwordBox.Password.ToString();
            string name = "";

            if ((bool)checkBoxCreateNewAcc.IsChecked)
            {
                name = usernameBox.Text;
                if (usersNames != null && usersNames.Contains(name)) {
                    displayErrors("user", "User with same name exist. Please choose other.");
                }
                else {
                    var data = createUser(name, password.ToString());
                    startApplication(data);
                }
            }
            else {
                name = selectUser.Text;
                var userData = getUserData(name, password);
                var isCorrectPassword = PasswordHasher.Verify(password, userData.credentials.Password);
                if (isCorrectPassword) {
                    startApplication(userData);
                }
                else {
                    displayErrors("password", "Incorrect password, try again please!");
                }
            }
        }

        /// <summary>
        /// Create user data from file.
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="password">User password</param>
        /// <returns>User data entity</returns>
        private UserData getUserData(string name, string password)
        {
            var filePath = DIRECTORY_PATH + '/' + name + ".txt";
            var encriptedString = File.ReadAllText(filePath);
            var xmlString = StringEncription.Decrypt(encriptedString, password);

            return SerializationManager.DeserializeUserData(xmlString);
        }

        /// <summary>
        /// Start main page and sent password as parameter.
        /// </summary>
        /// <param name="password">Password</param>
        private void startApplication(UserData data)
        {
            MainWindow main = new MainWindow(data);
            main.Show();
            Close();
        }

        /// <summary>
        /// Display error if name exist or password is wrong.
        /// </summary>
        /// <param name="errorType">Describe wrong field</param>
        /// <param="textError">Text to display for user</param>
        private void displayErrors(string errorType, string textError)
        {
            errorLabel.Content = textError;
            errorLabel.Foreground = System.Windows.Media.Brushes.Red;
            errorLabel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// If user apply for create new user, the textBox for input name is displayed.
        /// </summary>
        /// <param name="sender">Object item</param>
        /// <param name="e">Event</param>
        private void AddNewUserAction(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            if ((bool)checkBox.IsChecked) {
                selectUser.Visibility = Visibility.Collapsed;
                usernameBox.Visibility = Visibility.Visible;
                usernameBox.Text = "";
                passwordBox.Password = "";
                errorLabel.Visibility = Visibility.Collapsed;
            }
            else {
                selectUser.Visibility = Visibility.Visible;
                usernameBox.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// If in application folder exist user's files, it will be created a combobox with the names as choises.
        /// /</summary>
        /// <param name="sender">Object item</param>
        /// <param name="e">Event</param>
        private void LoginComboboxLoaded(object sender, RoutedEventArgs e)
        {
            selectUser = sender as ComboBox;
            if (usersNames != null && usersNames.Count != 0) {
                selectUser.ItemsSource = usersNames;
                selectUser.SelectedIndex = 0;
                selectUser.Visibility = Visibility.Visible;
                usernameBox.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Function initialize user names list.
        /// </summary>
        /// <param name="files">Files from application directory</param>
        private void setUsersNames(string[] files)
        {
            foreach (var filePath in files) {
                var fileName = Path.GetFileName(filePath);
                var username = Path.ChangeExtension(fileName, null);
                usersNames.Add(username);
            }
        }
        
        /// <summary>
        /// Creating a user consist in creating a file.
        /// In this file will be stored passwords.
        /// </summary>
        /// <param name="name">User's input as name</param>
        /// <param name="password">User's password</param>
        /// <returns>Path to created file</returns>
        private UserData createUser(string name, string password)
        {
            UserCredentials credentials = new UserCredentials();
            credentials.Name = name;
            credentials.Password = getPasswordHash(password);
            UserData data = new UserData();
            data.credentials = credentials;

            var xmlString = SerializationManager.SerializeUserData(data);
            string filePath = "";

            if (xmlString != null && !xmlString.Equals("")) {
                var encriptedString = StringEncription.Encrypt(xmlString, password);
                filePath = DIRECTORY_PATH + '/' + name + ".txt";
                File.WriteAllText(filePath, encriptedString);
            }
            return data;
        }

        /// <summary>
        /// Funtcion hash password and return hash to use as part of the filename.
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns>Password hash</returns>
        private string getPasswordHash(string password)
        {
            return PasswordHasher.Hash(password, 1000);
        }

        /// <summary>
        /// Function create application directory.
        /// </summary>
        private string[] checkStorageForFiles()
        {
            if (Directory.Exists(DIRECTORY_PATH)) {
                return Directory.GetFiles(DIRECTORY_PATH).OrderByDescending(d => new FileInfo(d).LastAccessTime).ToArray();
            }
            else {
                DirectoryInfo di = Directory.CreateDirectory(DIRECTORY_PATH);
                return new string[0];
            }
        }

        /// <summary>
        /// Handle TextBox mouse click to hide error label if is displayed.
        /// </summary>
        private void TextBox_MouseDown(object sender, RoutedEventArgs e)
        {
            if (errorLabel.Visibility == Visibility.Visible) {
                errorLabel.Visibility = Visibility.Collapsed;
            }
        }
    }
}

