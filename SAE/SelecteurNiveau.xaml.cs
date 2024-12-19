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
using System.Windows.Threading;
using static System.Formats.Asn1.AsnWriter;

namespace SAE
{
    /// <summary>
    /// Logique d'interaction pour SelecteurNiveau.xaml
    /// </summary>
    public partial class SelecteurNiveau : Window
    {

        public int niveauSelectionne {  get; set; }
        private static DispatcherTimer tick;
        private static int FPS;

        public SelecteurNiveau()
        {
            InitializeComponent();
            InitTimer();
            textBlockScore.Text = $"{MainWindow.score}";
        }

        private void InitTimer()
        {
            tick = new DispatcherTimer();
            tick.Interval = TimeSpan.FromMilliseconds(FPS);
            tick.Tick += Jeu;
            tick.Start();
        }

        private void Jeu(object sender, EventArgs e)
        {
            Console.WriteLine(MainWindow.score);
            if (MenuDemarrage.ModeDeJeu == 2)
            {
                textBlockScore.Text = $"{MainWindow.score}";
                textBlockScore.Visibility = Visibility.Visible;
            }
            else 
            {
                textBlockScore.Visibility = Visibility.Hidden;
            }
        }
        private void Bouton1_Click(object sender, RoutedEventArgs e)
        {
            niveauSelectionne = 1;
            Close();
        }

        private void Bouton2_Click(object sender, RoutedEventArgs e)
        {
            niveauSelectionne = 2;
            Close();
        }

        private void Bouton3_Click(object sender, RoutedEventArgs e)
        {
            niveauSelectionne = 3;
            Close();
        }

        private void BoutonAnnuler_Click(object sender, RoutedEventArgs e)
        {
            niveauSelectionne = 0;
            Close();
        }
    }
}
