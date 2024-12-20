﻿using System.Text;
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
using System.Windows.Threading;

namespace SAE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MenuDemarrage : Window
    {
        private static readonly int AJUSTEMENTVOLUME = 10;

        public static int ModeDeJeu { get; set;}

        private MediaPlayer musique;
        public MenuDemarrage()
        {
            InitializeComponent();

            // Démarrer les animations
            Loaded += MenuDemarrage_Chargée;

            // Initialisation du lecteur média
            musique = new MediaPlayer();

            // Chemin vers la musique (exemple : musique.mp3 dans le dossier Musiques)
            musique.Open(new Uri("music/Main Menu.mp3", UriKind.Relative));

            // Abonnement à l'événement de changement de volume
            Parametre.changementVolume += MajVolume;
            Console.WriteLine($"Valeur du volume récupéré: {musique.Volume}");

            // Lecture en boucle
            musique.MediaEnded += (s, e) =>
            {
                musique.Position = TimeSpan.Zero;
                musique.Play();
            };

            musique.Play();
        }

        private void MajVolume(double volume)
        {
            musique.Volume = volume / AJUSTEMENTVOLUME; // Mettre à jour le volume en temps réel
            Console.WriteLine($"Volume mis à jour : {musique.Volume}");
        }

        private void MenuDemarrage_Chargée(object sender, RoutedEventArgs e)
        {
            // Obtenir les dimensions réelles de l'image
            RotateTransform luneTransform = (RotateTransform)imgLune.RenderTransform;
            luneTransform.CenterX = imgLune.ActualWidth / 2;
            luneTransform.CenterY = imgLune.ActualHeight / 2;

            // Animation de la lune
            Storyboard moonStoryboard = (Storyboard)FindResource("RotateMoonStoryboard");
            moonStoryboard.Begin();

            // Animation de l'astronaute
            Storyboard astroStoryboard = (Storyboard)FindResource("MoveAstronautStoryboard");
            astroStoryboard.Begin();
        }

        private void butJouer_Click(object sender, RoutedEventArgs e)
        {
            // Arret de la musique actuelle
            musique.Stop();

            this.DialogResult = true;

            ModeDeJeu = 1;

            // Ferme la fenêtre actuelle (MenuDemarrage)
            this.Close();
        }

        private void butParametre_Click(object sender, RoutedEventArgs e)
        {
            Parametre dialog = new Parametre();
            bool? result = dialog.ShowDialog();
        }

        private void butQuitter_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void butInfini_Click(object sender, RoutedEventArgs e)
        {
            // Arret de la musique actuelle
            musique.Stop();

            this.DialogResult = true;

            ModeDeJeu = 2;

            // Ferme la fenêtre actuelle (MenuDemarrage)
            this.Close();
        }
    }
}