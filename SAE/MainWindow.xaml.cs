using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace SAE
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public bool pause, gauche, droite, haut , bas = false;
        private static DispatcherTimer tick, temps;


        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            this.MouseMove += DeplacementSouris;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            gauche = false;
            droite = false;
            haut = false;
            bas = false;
        }

        private void InitTimer()
        {
            tick = new DispatcherTimer();
            tick.Interval = TimeSpan.FromMilliseconds(16);
            tick.Tick += Jeu;
            tick.Start();

        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    haut = true;
                    break;

                case Key.Down:
                    bas = true;
                    break;

                case Key.Left:
                    gauche = true;
                    break;

                case Key.Right:
                    droite = true;
                    break;
            }
        }

        private void Jeu(object sender, EventArgs e)
        {
            if (!pause)
            {
                
                DeplacementCosmo();
            }
        }

        private void DeplacementSouris(object sender, MouseEventArgs e)
        {
            System.Windows.Point position = e.GetPosition(this); // Récupère la position de la souris par rapport à la fenêtre
            /* Console.WriteLine($"X: {position.X}, Y: {position.Y}"); */ // Affiche les coordonnées dans la fenêtre (par exemple dans une TextBox ou Console)

            // Récupère les coordonnées du centre du rectangle
            double centreCosmoX = Canvas.GetLeft(cosmo) + cosmo.Width / 2;
            double centreCosmoY = Canvas.GetTop(cosmo) + cosmo.Height / 2;

            // Calcule la différence entre la position de la souris et le centre du rectangle
            double deltaX = position.X - centreCosmoX;
            double deltaY = position.Y - centreCosmoY;

            // Calcule l'angle en utilisant la fonction Atan2 (pour gérer tous les quadrants)
            double angle = Math.Atan2(deltaY, deltaX) * (180 / Math.PI) + 90; // Conversion en degrés et ajustement de l'angle

            cosmo.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);  // Point d'origine au centre du rectangle

            // Créer un objet RotateTransform avec l'angle calculé et le centre comme origine de la rotation
            RotateTransform Rotation = new RotateTransform(angle);

            // Appliquer la transformation de rotation au rectangle
            cosmo.RenderTransform = Rotation;
        }

        private void DeplacementCosmo()
        {
            if (gauche && droite)
            {
                Console.WriteLine("gauche et droite");
            }
            else if (gauche)
            {
                RotateTransform rotateTransform = cosmo.RenderTransform as RotateTransform;
                if (rotateTransform != null)
                {
                    double angle = rotateTransform.Angle;

                    // Calcule le déplacement en fonction de l'angle
                    double vitesse = 10; // La vitesse de déplacement

                    // Convertir l'angle en radians
                    double angleRadians = angle * Math.PI / 180;

                    // Calculer le déplacement horizontal (gauche) et vertical
                    double deltaX = vitesse * Math.Cos(angleRadians);
                    double deltaY = vitesse * Math.Sin(angleRadians);

                    // Récupère la position actuelle du rectangle
                    double currentLeft = Canvas.GetLeft(cosmo);
                    double currentTop = Canvas.GetTop(cosmo);

                    // Déplace le rectangle en fonction de l'angle
                    Canvas.SetLeft(cosmo, currentLeft - deltaX);
                    Canvas.SetTop(cosmo, currentTop - deltaY);
                }
            }
            else if (droite)
            {
                RotateTransform rotateTransform = cosmo.RenderTransform as RotateTransform;
                if (rotateTransform != null)
                {
                    double angle = rotateTransform.Angle;

                    // Calcule le déplacement en fonction de l'angle
                    double vitesse = 10; // La vitesse de déplacement

                    // Convertir l'angle en radians
                    double angleRadians = angle * Math.PI / 180;

                    // Calculer le déplacement horizontal (gauche) et vertical
                    double deltaX = vitesse * Math.Cos(angleRadians);
                    double deltaY = vitesse * Math.Sin(angleRadians);

                    // Récupère la position actuelle du rectangle
                    double currentLeft = Canvas.GetLeft(cosmo);
                    double currentTop = Canvas.GetTop(cosmo);

                    // Déplace le rectangle en fonction de l'angle
                    Canvas.SetLeft(cosmo, currentLeft + deltaX);
                    Canvas.SetTop(cosmo, currentTop + deltaY);
                }
            }
            if (haut && bas)
            {
                Console.WriteLine("haut et bas");
            } 
            else if (haut)
            {
                // Récupérer la position actuelle de la souris relative au Canvas
                System.Windows.Point mousePosition = Mouse.GetPosition(this);

                // Récupérer la position actuelle de l'objet
                double currentLeft = Canvas.GetLeft(cosmo);
                double currentTop = Canvas.GetTop(cosmo);

                // Calculer le vecteur de déplacement vers la souris
                double deltaX = mousePosition.X - currentLeft;
                double deltaY = mousePosition.Y - currentTop;

                // Calculer la distance vers la souris
                double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

                // Normaliser le vecteur de déplacement pour un mouvement constant
                double vitesse = 10; // Vitesse de déplacement par tick
                if (distance > 0) // Éviter une division par zéro
                {
                    deltaX = (deltaX / distance) * vitesse;
                    deltaY = (deltaY / distance) * vitesse;
                }

                // Appliquer le déplacement
                Canvas.SetLeft(cosmo, currentLeft + deltaX);
                Canvas.SetTop(cosmo, currentTop + deltaY);
            }
            else if (bas)
            {
                Console.WriteLine("bas");
            }
            // Console.WriteLine($"gauche: {gauche}, droite: {droite}, bas: {bas}, haut: {haut}");
        }
    }
}
