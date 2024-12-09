using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SAE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MenuDemarrage : Window
    {
        public MenuDemarrage()
        {
            InitializeComponent();

            // Démarre les animations
            Loaded += MenuDemarrage_Loaded; //Appel de la methode qui dit que la fenetre a été chargé
        }

        private void MenuDemarrage_Loaded(object sender, RoutedEventArgs e)
        {
            // Obtenir les dimensions réelles de l'image
            var luneTransform = (RotateTransform)imgLune.RenderTransform;
            luneTransform.CenterX = imgLune.ActualWidth / 2;
            luneTransform.CenterY = imgLune.ActualHeight / 2;

            // Lancer l'animation
            var moonStoryboard = (Storyboard)FindResource("RotateMoonStoryboard");
            moonStoryboard.Begin();

            // Lance l'animation de montée et descente pour imgAstro
            var astroStoryboard = (Storyboard)FindResource("MoveAstronautStoryboard");
            astroStoryboard.Begin();
        }

        private void butJouer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void butParametre_Click(object sender, RoutedEventArgs e)
        {
            Parametre dialog = new Parametre();
            bool? result = dialog.ShowDialog();
            if (result == true)
            {

            }

        }

        private void butQuitter_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}