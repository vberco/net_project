using System.Windows;
using System.Text.RegularExpressions;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;

namespace proiect_pass_storage {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public UserData data { get; set; }
        public List<UserResourse> resourses { get; set; }
        private const int PASSWORD_MIN_LENGTH = 8;
        private const int PASSWORD_MAX_LENGTH = 30;
        private const int NON_ALFANUMERIC_CHARACTER_LENGTH = 2;
        private string password;
        private NotifyIcon NotifyIcon;


        /// <summary>
        /// Empty constructor.
        /// </summary>
        public MainWindow() {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with parameters.
        /// </summary>
        /// <param name="data">UserData entity with resourses.</param>
        /// <param name="password">Password used as salt for encription.</param>
        public MainWindow(UserData data, string password) {
            InitializeComponent();
            this.data = data;
            this.password = password;
            buttonCreateResourse.Click += ButtonCreateResourse_Click;
            buttonSearchPassword.Click += ButtonSearchPassword_Click;
            buttonGeneratePassword.Click += ButtonGeneratePassword_Click;
            buttonSaveResourse.Click += ButtonSaveResourseClick_Click;
            buttonSearchResourse.Click += ButtonSearchResourse_Click;
            buttonDeletePassword.Click += ButtonDeletePassword_Click;
            buttonUpdatePassword.Click += ButtonUpdatePassword_Click;
            buttonViewAll.Click += ButtonViewAll_Click;

            gridCreatePassword.Visibility = Visibility.Collapsed;
            gridSearchPassword.Visibility = Visibility.Collapsed;
            gridViewAllResourses.Visibility = Visibility.Collapsed;
            this.StateChanged += MainWindow_StateChanged;           
            DataContext = this;
            initializeTray();
        }

       private void initializeTray() {
            NotifyIcon = new NotifyIcon();
            NotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(@"../../Images/key.jpg");
            NotifyIcon.MouseDoubleClick +=
                new System.Windows.Forms.MouseEventHandler
                    (NotifyIcon_MouseDoubleClick);
        }

        private void MainWindow_StateChanged(object sender, EventArgs e) {
            if (this.WindowState == WindowState.Minimized) {
                this.ShowInTaskbar = false;
                NotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                NotifyIcon.BalloonTipText = "Minimized the app ";
                NotifyIcon.ShowBalloonTip(400);
                NotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal) {
                NotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void ButtonViewAll_Click(object sender, RoutedEventArgs e) {
            gridViewAllResourses.Visibility = Visibility.Visible;
            gridCreatePassword.Visibility = Visibility.Collapsed;
            gridSearchPassword.Visibility = Visibility.Collapsed;
        }

        private void ButtonUpdatePassword_Click(object sender, RoutedEventArgs e) {
            var resourse = (UserResourse)listViewResourses.SelectedItem;
            resetSearchForm();
            gridSearchPassword.Visibility = Visibility.Collapsed;
            gridCreatePassword.Visibility = Visibility.Visible;
            textBoxInputResourse.Text = resourse.ResourseName;
        }

        /// <summary>
        /// Handler for delete password button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDeletePassword_Click(object sender, RoutedEventArgs e) 
        {
            var resourse =(UserResourse) listViewResourses.SelectedItem;
            if (resourse != null) {
                data.Resourses.Remove(resourse);
                resourses.Remove(resourse);
                listViewResourses.ItemsSource = null;
                listViewResourses.Items.Clear();
                updateDataInFile(data);
                listViewResourses.ItemsSource = resourses;
            }
        }
        /// <summary>
        /// Set all fields empty.
        /// </summary>
        private void resetSearchForm()
        {
            textBoxSearchResourse.Text = null;
            resourses = null;
            listViewResourses.ItemsSource = null;
            listViewResourses.Items.Clear();
        }

        /// <summary>
        /// Handler for search resourse button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonSearchResourse_Click(object sender, RoutedEventArgs e) {
            resourses = new List<UserResourse>();
            listViewResourses.ItemsSource = null;
            listViewResourses.Items.Clear();

            resourses = data.Resourses.FindAll(r => r.ResourseName.Contains(textBoxSearchResourse.Text));
            if (resourses.Count == 0) {
                labelNotFound.Content = "The resourse with such name not found!";
                labelNotFound.Visibility = Visibility.Visible;
                labelNotFound.Foreground = System.Windows.Media.Brushes.Red;
            }
            else {
                listViewResourses.ItemsSource = resourses;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
               
                var existedResourse = data.Resourses.Find(
                    r => r.ResourseName.Equals(resourse.ResourseName));
                if (existedResourse == null) {
                    data.Resourses.Add(resourse);
                    errorLabel.Content = "Resourse was created successfuly.";
                }
                else {
                    existedResourse.ResoursePassword = textBoxInputPassword.Text;
                    errorLabel.Content = "Resourse was updated successfuly.";
                }

                updateDataInFile(data);
                errorLabel.Visibility = Visibility.Visible;
                errorLabel.Foreground = System.Windows.Media.Brushes.Green;
                resetCreateForm();
            }
        }

        /// <summary>
        /// Reset create form fields to empty values.
        /// </summary>
        private void resetCreateForm()
        {
            textBoxInputResourse.Text = "";
            textBoxInputPassword.Text = "";
            textBoxPasswordLength.Text = "";
            textBoxNrSpecialSymbols.Text = "";
        }

        /// <summary>
        /// Rewrite file after update data.
        /// </summary>
        /// <param name="data">Modified user data.</param>
        private void updateDataInFile(UserData data) {
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
            gridViewAllResourses.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handle resourse create button click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCreateResourse_Click(object sender, RoutedEventArgs e) {
            gridCreatePassword.Visibility = Visibility.Visible;
            gridSearchPassword.Visibility = Visibility.Collapsed;
            gridViewAllResourses.Visibility = Visibility.Collapsed;
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

        private void listViewResourses_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {

        }

        /// <summary>
        /// On click event on list item, buttons become active.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listViewResourses_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var clickedResourse = (sender as System.Windows.Controls.ListView).SelectedItem;
            if (clickedResourse != null) {
                buttonDeletePassword.IsEnabled = true;
                buttonUpdatePassword.IsEnabled = true;
            }
        }
    }
}
