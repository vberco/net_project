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
        private Dictionary<string, string> usersNames;

        public AuthorizationPage()
        {
            InitializeComponent();
            var files = checkStorageForFiles();
            if (files.Count() == 0) {
                passInfoLabel.Content = "Create new user";
                checkBoxCreateNewAcc.Visibility = Visibility.Collapsed;// or:System.Windows.Visibility.Hidden
                checkBoxCreateNewAcc.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                selectUser.Visibility = Visibility.Collapsed;
                usernameBox.Visibility = Visibility.Visible;

            }
            else {
                usersNames = getUsernames(files);
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
                if (usersNames != null && usersNames.ContainsKey(name)) {
                    displayErrors("user", "User with same name exist. Please choose other.");
                }
                else {
                    var filePath = createUser(name, password.ToString());
                    startApplication(password.ToString(), filePath);
                }
            }
            else {
                name = selectUser.Text;
                var fileName = Path.GetFileName(usersNames[name]);
                var hash = Path.ChangeExtension(fileName.Split('_')[1], null);
                var isCorrectPassword = PasswordHasher.Verify(password, hash);
                if (isCorrectPassword) {
                    startApplication(password.ToString(), usersNames[name]);
                }
                else {
                    displayErrors("password", "Incorrect password, try again please!");
                }
            }
        }

        /// <summary>
        /// Start main page and sent password as parameter.
        /// </summary>
        /// <param name="password">Password</param>
        private void startApplication(string password, string filePath)
        {
            MainWindow main = new MainWindow(password.ToString(), filePath);
            main.Show();
            this.Close();
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
            selectUser.ItemsSource = usersNames.Keys;
            selectUser.SelectedIndex = 0;
            selectUser.Visibility = Visibility.Visible;
            usernameBox.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Function create a Dictionary with username as key and password hash as value.
        /// </summary>
        /// <returns> Created dictionary</returns>
        private Dictionary<string, string> getUsernames(string[] files)
        {
            Dictionary<string, string> usernames = new Dictionary<string, string>();
            foreach (String filePath in files) {
                var fileName = Path.GetFileName(filePath);
                var credentials = fileName.Split('_');
                usernames.Add(credentials[0], filePath);
            }

            return usernames;
        }
        
        /// <summary>
        /// Creating a user consist in creating a file.
        /// In this file will be stored passwords.
        /// </summary>
        /// <param name="name">User's input as name</param>
        /// <param name="password">User's password</param>
        /// <returns>Path to created file</returns>
        private string createUser(string name, string password)
        {
            var hash = getPasswordHash(password);
            string fileName = name + "_" + hash + ".txt";
            string filePath = DIRECTORY_PATH + '/' + fileName;
            FileInfo fi = new FileInfo(filePath);
            fi.Create();

            return filePath;
        }

        /// <summary>
        /// Funtcion hash password and return hash to use as part of the filename.
        /// </summary>
        /// <param name="password">Password to hash</param>
        /// <returns>Password hash</returns>
        private string getPasswordHash(string password)
        {
            string hash = "";
            Regex regex = null;
            bool isValidName = false;
            do {
                hash = PasswordHasher.Hash(password, 1000);
                isValidName = IsValidFilename(hash);
            } while (!isValidName);

            return hash;
        }

        /// <summary>
        /// Check if hash can be used as part of the file name.
        /// </summary>
        /// <param name="fileName">Generated hash</param>
        /// <returns>Is valid file name or not.</returns>
        private bool IsValidFilename(string fileName) {
            Regex containsABadCharacter = new Regex("["
                  + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
            return containsABadCharacter.IsMatch(fileName) ? false : true;
        }

        /// <summary>
        /// Function create application directory.
        /// </summary>
        private string[] checkStorageForFiles()
        {
            var directoryExist = Directory.Exists(DIRECTORY_PATH);

            if (directoryExist) {
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
