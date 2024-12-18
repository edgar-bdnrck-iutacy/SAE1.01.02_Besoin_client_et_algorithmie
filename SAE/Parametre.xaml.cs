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
        // Délégué pour notifier le changement de volume
        public static event Action<double> changementVolume;

        public Parametre()
        {
            InitializeComponent();

            slidVolume.Value = Volume; // Initialiser le slider avec la valeur actuelle
            slidVolume.ValueChanged += SlidVolume_ValueChanged;
        }

        private void SlidVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Volume = slidVolume.Value; // Mettre à jour la variable statique
            changementVolume?.Invoke(Volume); // Notifier les abonnés
            Console.WriteLine($"Valeur du slider volume: {Volume}");
        }

    }
}
