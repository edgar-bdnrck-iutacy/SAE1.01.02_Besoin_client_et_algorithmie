using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization.Metadata;
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
    public partial class MainWindow : Window
    {
        public bool pause = false, gauche = false, droite = false, haut = false, bas = false, enMouvement = false, lazerTire = false, lazerToucheCosmo = false;
        private static DispatcherTimer tick, temps;
        private static double distanceX = 0, distanceY = 0, vitesse = 2, vitesseLazer = 10, ticks = 0, trajectoireX, trajectoireY;
        private MediaPlayer musique;


        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            this.MouseMove += DeplacementSouris;

            //Init du lecteur de média pour la musique de niveaux
            musique = new MediaPlayer();
            musique.Open(new Uri("music/Level.mp3", UriKind.Relative));

            //Definit Volume par rapport au slider vu en paramètres
            musique.Volume = Parametre.Volume / 10;

            //Lecutre en boucle
            musique.MediaEnded += (s, e) =>
            {
                musique.Position = TimeSpan.Zero;
                musique.Play();
            };

            musique.Play();

            // Changement du volume en temps réel
            Parametre.changementVolume += MajVolume;
        }

        private void MajVolume(double volume)
        {
            musique.Volume = volume / 10; // Mise a jour du volume
            Console.WriteLine($"Volume mis a jour dans le MainWindow: {musique.Volume}");
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
            Console.WriteLine(e.Key);
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
            enMouvement = (droite || gauche || haut || bas) && !(droite && gauche) && !(haut && bas);
            if (!pause)
            {
                DeplacementCosmo();
                if (enMouvement)
                {
                    if (vitesse < 10)
                    {
                        vitesse = vitesse + 0.25;
                    }
                }
                else
                {
                    vitesse = 0;
                }
                if (!lazerTire)
                {
                    Canvas.SetLeft(lazer, Canvas.GetLeft(alien));
                    Canvas.SetTop(lazer, Canvas.GetTop(alien));
                    lazerTire = true;
                    lazerToucheCosmo = false;

                    distanceX = (Canvas.GetLeft(cosmo) + cosmo.Width / 2) - (Canvas.GetLeft(lazer) + lazer.Width / 2);
                    distanceY = (Canvas.GetTop(cosmo) + cosmo.Height / 2) - (Canvas.GetTop(lazer) + lazer.Height / 2);

                    // Calcule l'angle en utilisant la fonction Atan2
                    double angle = Math.Atan2(distanceY, distanceX) * (180 / Math.PI) + 90; // Conversion en degrés et ajustement de l'angle

                    lazer.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    // Créer un objet RotateTransform avec l'angle calculé et le centre comme origine de la rotation
                    RotateTransform Rotation = new RotateTransform(angle);

                    // Appliquer la transformation de rotation a cosmo
                    lazer.RenderTransform = Rotation;

                    double distanceXY = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                    trajectoireX = distanceX / distanceXY;
                    trajectoireY = distanceY / distanceXY;
                } 
                else
                {
                    TirLazer();
                }
            }
        }

        private void DeplacementSouris(object sender, MouseEventArgs e)
        {
            // Récupère la position de la souris par rapport à la fenêtre
            System.Windows.Point position = e.GetPosition(this);
            
            // Calcule la différence entre la position de la souris et le centre du rectangle
            distanceX = position.X - (Canvas.GetLeft(cosmo) + cosmo.Width / 2);
            distanceY = position.Y - (Canvas.GetTop(cosmo) + cosmo.Height / 2);

            // Calcule l'angle en utilisant la fonction Atan2
            double angle = Math.Atan2(distanceY, distanceX) * (180 / Math.PI) + 90; // Conversion en degrés et ajustement de l'angle

            cosmo.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

            // Créer un objet RotateTransform avec l'angle calculé et le centre comme origine de la rotation
            RotateTransform Rotation = new RotateTransform(angle);

            // Appliquer la transformation de rotation a cosmo
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

                    // Convertir l'angle en radians
                    double angleRadians = angle * Math.PI / 180;

                    // Calculer le déplacement horizontal (gauche) et vertical
                    distanceX = Math.Cos(angleRadians);
                    distanceY = Math.Sin(angleRadians);

                    // Déplace le rectangle en fonction de l'angle
                    Canvas.SetLeft(cosmo, Canvas.GetLeft(cosmo) - distanceX * vitesse);
                    Canvas.SetTop(cosmo, Canvas.GetTop(cosmo) - distanceY * vitesse);
                }
            }
            else if (droite)
            {
                RotateTransform rotateTransform = cosmo.RenderTransform as RotateTransform;
                if (rotateTransform != null)
                {
                    double angle = rotateTransform.Angle;

                    // Convertir l'angle en radians
                    double angleRadians = angle * Math.PI / 180;

                    // Calculer le déplacement horizontal (gauche) et vertical
                    distanceX = Math.Cos(angleRadians);
                    distanceY = Math.Sin(angleRadians);

                    // Déplace le rectangle en fonction de l'angle
                    Canvas.SetLeft(cosmo, Canvas.GetLeft(cosmo) + distanceX * vitesse);
                    Canvas.SetTop(cosmo, Canvas.GetTop(cosmo) + distanceY * vitesse);
                }
            }
            if (haut && bas)
            {
                Console.WriteLine("haut et bas");
            } 
            else if (haut)
            {
                // Récupérer la position actuelle de la souris relative au Canvas
                System.Windows.Point PositionSouris = Mouse.GetPosition(this);

                // Calculer le vecteur de déplacement vers la souris
                distanceX = PositionSouris.X - (Canvas.GetLeft(cosmo) + cosmo.Width / 2);
                distanceY = PositionSouris.Y - (Canvas.GetTop(cosmo) + cosmo.Height / 2);

                // Calculer la distance vers la souris avec le théoreme de pythagore
                double distanceXY = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                if (distanceXY > 30)
                {
                    distanceX = distanceX / distanceXY;
                    distanceY = distanceY / distanceXY;

                    Canvas.SetLeft(cosmo, Canvas.GetLeft(cosmo) + distanceX * vitesse);
                    Canvas.SetTop(cosmo, Canvas.GetTop(cosmo) + distanceY * vitesse);
                }
            }
            else if (bas)
            {
                
                // Récupérer la position actuelle de la souris relative au Canvas
                System.Windows.Point PositionSouris = Mouse.GetPosition(this);

                // Calculer le vecteur de déplacement vers la souris
                distanceX = PositionSouris.X - (Canvas.GetLeft(cosmo) + cosmo.Width / 2);
                distanceY = PositionSouris.Y - (Canvas.GetTop(cosmo) + cosmo.Height / 2);

                // Calculer la distance vers la souris avec le théoreme de pythagore
                double distance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

                if (distance > 0)
                {
                    distanceX = -distanceX / distance;
                    distanceY = -distanceY / distance;
                }
                Canvas.SetLeft(cosmo, Canvas.GetLeft(cosmo) + distanceX* vitesse);
                Canvas.SetTop(cosmo, Canvas.GetTop(cosmo) + distanceY * vitesse);
            }
        }
        private void TirLazer()
        {
            Canvas.SetLeft(lazer, Canvas.GetLeft(lazer) + trajectoireX * vitesseLazer);
            Canvas.SetTop(lazer, Canvas.GetTop(lazer) + trajectoireY * vitesseLazer);

            if ((Canvas.GetTop(lazer) + lazer.Height) > Canvas.GetTop(cosmo) && Canvas.GetTop(lazer) < (Canvas.GetTop(cosmo) + cosmo.Height) && ((Canvas.GetLeft(lazer) + lazer.Width) > Canvas.GetLeft(cosmo) && Canvas.GetLeft(lazer) < (Canvas.GetLeft(cosmo) + cosmo.Width)))

            {
                lazerTire = false;
                lazerToucheCosmo = true;
            }
            
            else if (CollisionAvecBord(canvas,lazer)) 
            {
                lazerTire = false;
            }
            
        }
        private bool CollisionAvecBord(Canvas canvas, UIElement element)
        {
            // Récupérer la position et les dimensions de l'élément
            double left = Canvas.GetLeft(element);
            double top = Canvas.GetTop(element);
            double width = element.RenderSize.Width;
            double height = element.RenderSize.Height;

            // Récupérer les dimensions du Canvas
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            // Vérifier les collisions avec les bords
            bool toucheGauche = left <= 0;
            bool toucheDroite = left + width >= canvasWidth;
            bool toucheHaut = top <= 0;
            bool toucheBas = top + height >= canvasHeight;

            // Retourner vrai si une collision est détectée
            return toucheGauche || toucheDroite || toucheHaut || toucheBas;
        }
    }
 }

