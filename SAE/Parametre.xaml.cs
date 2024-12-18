using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SAE
{
    /// <summary>
    /// Logique d'interaction pour Parametre.xaml
    /// </summary>
 
    public partial class Parametre : Window
    {
        // Variable statique pour stocker le volume
        public static double Volume { get; set; } = 50; // Valeur par défaut 100%
        // Notifier le changement de volume
        public static event Action<double> changementVolume;

        public static string KeyAvancer { get; set; } = "Z";
        public static string KeyReculer { get; set; } = "S";
        public static string KeyGauche { get; set; } = "Q";
        public static string KeyDroite { get; set; } = "D";
        public static string KeyPause { get; set; } = "P";

        public Parametre()
        {
            InitializeComponent();

            //Volume
            slidVolume.Value = Volume; // Initialiser le slider avec la valeur actuelle
            slidVolume.ValueChanged += SlidVolume_ValueChanged;

            //Changement des commandes
            // Initialise les valeurs des TextBox
            txtAvancer.Text = KeyAvancer;
            txtReculer.Text = KeyReculer;
            txtGauche.Text = KeyGauche;
            txtDroite.Text = KeyDroite;
            txtPause.Text = KeyPause;

            // Quand LostFocus sauvegarde des nouvelles touches
            txtAvancer.LostFocus += (s, e) => KeyAvancer = txtAvancer.Text.ToUpper();
            txtReculer.LostFocus += (s, e) => KeyReculer = txtReculer.Text.ToUpper();
            txtGauche.LostFocus += (s, e) => KeyGauche = txtGauche.Text.ToUpper();
            txtDroite.LostFocus += (s, e) => KeyDroite = txtDroite.Text.ToUpper();
            txtPause.LostFocus += (s, e) => KeyPause = txtPause.Text.ToUpper();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                textBox.Text = string.Empty; // Efface le contenu lors du focus
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                int debutSelection = textBox.SelectionStart; // Sauvegarde de la position du curseur
                textBox.Text = textBox.Text.ToUpper(); // Conversion en majuscules
                textBox.SelectionStart = debutSelection; // Restauration de la position du curseur
            }
        }

        private void SlidVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = slidVolume.Value; // Mettre à jour la variable statique
            changementVolume?.Invoke(Volume); // Notifier les abonnés
            Console.WriteLine($"Valeur du slider volume: {Volume}");
        }

    }
}
