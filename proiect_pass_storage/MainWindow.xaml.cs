using System.Windows;
using System.Text.RegularExpressions;
using System;
using System.IO;

namespace proiect_pass_storage {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private UserData data;
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 30;
        private const int NON_ALFANUMERIC_CHARACTER_LENGTH = 2;
        private string password;
        public MainWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public MainWindow(UserData data, string password) {
            InitializeComponent();
            this.data = data;
            this.password = password;
            buttonCreateResourse.Click += ButtonCreateResourse_Click;
            buttonSearchPassword.Click += ButtonSearchPassword_Click;
            buttonGeneratePassword.Click += ButtonGeneratePassword_Click;
            buttonSaveResourse.Click += ButtonSaveResourseClick_Click;

            DataContext = this;
        }

        private void ButtonSaveResourseClick_Click(object sender, RoutedEventArgs e)
        {
            string textError = "";
            if (textBoxInputResourse.Text.Equals("")) {
                textError = "Resourse name cannot be empty.\n";
            }
            if (textBoxInputPassword.Text.Equals("")) {
                textError += "Password field cannot be empty.";
            }
            
            if (!textError.Equals("")) {
                errorLabel.Content = textError;
                errorLabel.Foreground = System.Windows.Media.Brushes.Red;
                errorLabel.Visibility = Visibility.Visible;
            }
            else {
                UserResourse resourse = new UserResourse();
                resourse.ResourseName = textBoxInputResourse.Text;
                resourse.ResoursePassword = textBoxInputPassword.Text;
                data.Resourses.Add(resourse);
                updateData(data);
            }
        }

        private void updateData(UserData data) {
            var serializedData = new SerializationManager().SerializeUserData(data);
            var encriptedString = StringEncription.Encrypt(serializedData, password);
            var filePath = data.Credentials.Name;
            File.WriteAllText(filePath, encriptedString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonGeneratePassword_Click(object sender, RoutedEventArgs e) {
            var userPasswordLength = textBoxPasswordLength.Text;
            int passwordLength = PASSWORD_MAX_LENGTH;

            if (!userPasswordLength.Equals("")) {
                int length;
                if (int.TryParse(userPasswordLength, out length)) {
                    if (length >= PASSWORD_MIN_LENGTH && length <= PASSWORD_MAX_LENGTH) {
                        passwordLength = length;
                    }
                }
            }       
               
            int nonAlfaNumericLength = NON_ALFANUMERIC_CHARACTER_LENGTH;
            var userNonAlfaNumericLength = textBoxNrSpecialSymbols.Text;
            if (!userNonAlfaNumericLength.Equals("")) {
                int length;
                if (int.TryParse(userNonAlfaNumericLength, out length)) {
                    if (passwordLength - length > 3) {
                        nonAlfaNumericLength = length;
                    }
                }
            }
            var generatedPassword = generatePassword(passwordLength, nonAlfaNumericLength);
            textBoxInputPassword.Text = generatePassword(passwordLength, nonAlfaNumericLength);
        }

        /// <summary>
        /// Generate password while it match regular expression.
        /// </summary>
        /// <param name="passwordLength"></param>
        /// <param name="nonAlfaNumericLength"></param>
        /// <returns></returns>
        private string generatePassword(int passwordLength, int nonAlfaNumericLength) {
            string password = "";
            string regexString = @"^(?=(.*[^A-Za-z0-9]){2})(?=(.*[A-Z]){2})(?=(.*[a-z]){2})(?=(.*\d){" + nonAlfaNumericLength + "}).+";
            var regex = new Regex(regexString);
            do {
                password = getRandomPassword(passwordLength);
            } while (!regex.IsMatch(password));

            return password;
        }

        /// <summary>
        /// Create random password.
        /// </summary>
        /// <param name="length">Password length</param>
        /// <returns>Created password</returns>
        private string getRandomPassword(int length) {
            string password = "";
            var random = new Random((int)DateTime.Now.Ticks);
            try {
                byte[] result = new byte[length];
                for (int index = 0; index < length; index++) {
                    result[index] = (byte)random.Next(33, 126);
                }
                password = System.Text.Encoding.ASCII.GetString(result);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message, ex);
            }

            return password;
        }
        /// <summary>
        /// Handle search password button click. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSearchPassword_Click(object sender, RoutedEventArgs e) {
            gridCreatePassword.Visibility = Visibility.Collapsed;
            gridSearchPassword.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handle resourse create button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCreateResourse_Click(object sender, RoutedEventArgs e) {
            gridCreatePassword.Visibility = Visibility.Visible;
            gridSearchPassword.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handle TextBox mouse click to hide error label if is displayed.
        /// </summary>
        private void TextBox_MouseDown(object sender, RoutedEventArgs e) {
            
            if (errorLabel.Visibility == Visibility.Visible || labelNotFound.Visibility == Visibility.Visible) {
                errorLabel.Visibility = Visibility.Collapsed;
                labelNotFound.Visibility = Visibility.Collapsed;
            }
        }
    }
}
