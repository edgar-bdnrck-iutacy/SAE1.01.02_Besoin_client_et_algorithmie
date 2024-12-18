﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;

namespace SAE
{
    public partial class MainWindow : Window
    {
        public static readonly int SCOREMAX1 = 10, SCOREMAX2 = 15, SCOREMAX3 = 25;
        public bool dejaAppele = false, versDroite = true, lobby = true, pause = false, interaction = false, gauche = false, droite = false, haut = false, bas = false, enMouvement = false, lazerTire = false, lazerToucheCosmo = false;
        private static DispatcherTimer tick, temps;
        private static double distanceX = 0, distanceY = 0, vitesse = 2, vitessemax = 10, vitesseLazer = 15, vitesseAlien = 5, ticks = 0, trajectoireX, trajectoireY, trajectoireX_2, trajectoireY_2;
        private static int score = 0, niveau = 1, scoreMax = 0;
        private MediaPlayer musique;
        private Random random = new Random();


        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
            InitialiseLobby();
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
            switch (e.Key)
            {
                case Key.Up:
                case Key.Z:
                    haut = false;
                    break;

                case Key.Down:
                case Key.S:
                    bas = false;
                    break;

                case Key.Left:
                case Key.Q:
                    gauche = false;
                    break;

                case Key.Right:
                case Key.D:
                    droite = false;
                    break;

                case Key.Space:
                    interaction = false;
                    break;

                // code de triche
                case Key.O:
                    score++;
                    break;
                case Key.I:
                    vitessemax = 30;
                    break;
            }
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
                case Key.Z:
                    haut = true;
                    break;

                case Key.Down:
                case Key.S:
                    bas = true;
                    break;

                case Key.Left:
                case Key.Q:
                    gauche = true;
                    break;

                case Key.Right:
                case Key.D:
                    droite = true;
                    break;

                case Key.P:
                    pause = !pause;
                    break;

                case Key.Space:
                    interaction = true;
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
                    if (vitesse < vitessemax)
                    {
                        vitesse = vitesse + 0.25;
                    }
                }
                else
                {
                    vitesse = 0;
                }
                if (!lobby)
                {
                    if (!dejaAppele)
                        InitialiseNiv();
                    
                    if (niveau == 2)
                    {
                        if (versDroite)
                        {
                            Canvas.SetLeft(alien_2, Canvas.GetLeft(alien_2) + vitesseAlien);
                            if (Canvas.GetLeft(alien_2) > 800)
                            {
                                versDroite = false;
                            }
                        }
                        else
                        {
                            Canvas.SetLeft(alien_2, Canvas.GetLeft(alien_2) - vitesseAlien);
                            if (Canvas.GetLeft(alien_2) < 50)
                            {
                                versDroite = true;
                            }
                        }
                    } 
                    else if (niveau == 3)
                    {
                        Canvas.SetLeft(lazer_2, Canvas.GetLeft(alien));
                        InitialiseTrajectoire3();
                        DeplacementAlien();
                    }
                    if (score == scoreMax)
                    {
                        dejaAppele = false;
                        niveau++;
                        lobby = true;
                    }
                    if (CollisionEntreEntité(cosmo, satellite))
                    {
                        score++;
                        labelScore.Content = $"{score}/{scoreMax}";
                        do
                        {
                            Canvas.SetLeft(satellite, random.Next(0, (int)(canvas.ActualWidth - satellite.Width)));
                            Canvas.SetTop(satellite, random.Next(0, (int)(canvas.ActualHeight - satellite.Height)));
                        }
                        while (CollisionEntreEntité(alien, satellite));
                    }

                    if (!lazerTire)
                    {
                        InitialiseTrajectoire();
                        if (niveau == 2)
                            InitialiseTrajectoire2();
                    }
                    else
                    {
                        TirLazer();
                        if (niveau == 2)
                            TirLazer2();
                    }

                    if (CollisionEntreEntité(lazer, cosmo) || CollisionEntreEntité(lazer_2, cosmo) || CollisionEntreEntité(alien, cosmo) || CollisionEntreEntité(alien_2, cosmo))
                    {
                        Canvas.SetLeft(LabelGameOver, canvas.ActualWidth / 2);
                        Canvas.SetTop(LabelGameOver, canvas.ActualHeight / 2);
                        LabelGameOver.Visibility = Visibility.Visible;
                        lobby = true;
                        score = 0;
                        labelScore.Content = $"{score}/{scoreMax}";
                        dejaAppele = false;
                        Canvas.SetLeft(cosmo, canvas.ActualWidth / 2);
                        Canvas.SetTop(cosmo, canvas.ActualHeight / 2);

                    }
                    else
                    {
                        LabelGameOver.Visibility = Visibility.Hidden;
                    }
                }
                else 
                {
                    InitialiseLobby();
                    if (CollisionEntreEntité(cosmo,Fusee))
                    {
                        
                        if (interaction)
                        {
                            lobby = false;
                        }
                    }
                }
            }
        }

        private void InitialiseNiv()
        {
            SolLunaire.Visibility = Visibility.Hidden;
            Fusee.Visibility = Visibility.Hidden;
            labelScore.Visibility = Visibility.Visible;
            satellite.Visibility = Visibility.Visible;
            alien.Visibility = Visibility.Visible;
            lazer.Visibility = Visibility.Visible;
            dejaAppele = true;

            if (niveau == 1)
            {
                scoreMax = SCOREMAX1;
                Canvas.SetLeft(alien, 360);
                Canvas.SetTop(alien, 269);
            }
            else if (niveau == 2)
            {
                lazer_2.Visibility = Visibility.Visible;
                alien_2.Visibility = Visibility.Visible;
                scoreMax = SCOREMAX2;
                Canvas.SetLeft(alien_2, 50);
                Canvas.SetTop(alien_2, 186);
            }
            else if (niveau == 3)
            {
                alien_2.Visibility = Visibility.Visible;
                scoreMax = SCOREMAX3;
            }
        }

        private void InitialiseLobby()
        {
            labelScore.Visibility = Visibility.Hidden;
            alien.Visibility = Visibility.Hidden;
            lazer.Visibility = Visibility.Hidden;
            lazer_2.Visibility = Visibility.Hidden;
            satellite.Visibility = Visibility.Hidden;
            alien_2.Visibility = Visibility.Hidden;

            SolLunaire.Visibility = Visibility.Visible;
            Fusee.Visibility = Visibility.Visible;
        }

        private void DeplacementSouris(object sender, MouseEventArgs e)
        {
            if (!pause)
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
        }

        private void InitialiseTrajectoire()
        {
            Canvas.SetLeft(lazer, Canvas.GetLeft(alien) + alien.Width / 2);
            Canvas.SetTop(lazer, Canvas.GetTop(alien) + alien.Height / 2);
            lazerTire = true;
            lazerToucheCosmo = false;

            distanceX = (Canvas.GetLeft(cosmo) + cosmo.Width / 2) - (Canvas.GetLeft(lazer) + lazer.Width / 2);
            distanceY = (Canvas.GetTop(cosmo) + cosmo.Height / 2) - (Canvas.GetTop(lazer) + lazer.Height / 2);

            // Calcule l'angle en utilisant la fonction Atan2
            double angle = Math.Atan2(distanceY, distanceX) * (180 / Math.PI) + 180; // Conversion en degrés et ajustement de l'angle

            lazer.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

            // Créer un objet RotateTransform avec l'angle calculé et le centre comme origine de la rotation
            RotateTransform Rotation = new RotateTransform(angle);

            // Appliquer la transformation de rotation a cosmo
            lazer.RenderTransform = Rotation;

            double distanceXY = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            trajectoireX = distanceX / distanceXY;
            trajectoireY = distanceY / distanceXY;
        }

        private void InitialiseTrajectoire2()
        {
            Canvas.SetLeft(lazer_2, Canvas.GetLeft(alien_2) + alien.Width / 2);
            Canvas.SetTop(lazer_2, Canvas.GetTop(alien_2) + alien.Height / 2);
            lazerTire = true;
            lazerToucheCosmo = false;

            distanceX = (Canvas.GetLeft(cosmo) + cosmo.Width / 2) - (Canvas.GetLeft(lazer_2) + lazer.Width / 2);
            distanceY = (Canvas.GetTop(cosmo) + cosmo.Height / 2) - (Canvas.GetTop(lazer_2) + lazer.Height / 2);

            // Calcule l'angle en utilisant la fonction Atan2
            double angle = Math.Atan2(distanceY, distanceX) * (180 / Math.PI) + 180; // Conversion en degrés et ajustement de l'angle

            lazer.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

            // Créer un objet RotateTransform avec l'angle calculé et le centre comme origine de la rotation
            RotateTransform Rotation = new RotateTransform(angle);

            // Appliquer la transformation de rotation a cosmo
            lazer_2.RenderTransform = Rotation;

            double distanceXY = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            trajectoireX_2 = distanceX / distanceXY;
            trajectoireY_2 = distanceY / distanceXY;
        }

        private void InitialiseTrajectoire3()
        {
            distanceX = (Canvas.GetLeft(cosmo) + cosmo.Width / 2) - (Canvas.GetLeft(alien_2) + alien_2.Width / 2);
            distanceY = (Canvas.GetTop(cosmo) + cosmo.Height / 2) - (Canvas.GetTop(alien_2) + alien_2.Height / 2);

            double distanceXY = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            trajectoireX_2 = distanceX / distanceXY;
            trajectoireY_2 = distanceY / distanceXY;
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


            if (CollisionEntreEntité(lazer, cosmo))
            {
                lazerTire = false;
            } 
            if (CollisionAvecBord(canvas,lazer)) 
            {
                lazerTire = false;
            }
        }

        private void TirLazer2()
        {
            Canvas.SetLeft(lazer_2, Canvas.GetLeft(lazer_2) + trajectoireX_2 * vitesseLazer);
            Canvas.SetTop(lazer_2, Canvas.GetTop(lazer_2) + trajectoireY_2 * vitesseLazer);


            if (CollisionEntreEntité(lazer_2,cosmo))
            {
                lazerTire = false;
            }
            if (CollisionAvecBord(canvas, lazer))
            {
                lazerTire = false;
            }
        }

        private void DeplacementAlien()
        {
            Canvas.SetLeft(alien_2, Canvas.GetLeft(alien_2) + trajectoireX_2 * 3);
            Canvas.SetTop(alien_2, Canvas.GetTop(alien_2) + trajectoireY_2 * 3);

            if (CollisionEntreEntité(cosmo, alien_2))
            {
                Canvas.SetLeft(alien_2, Canvas.GetLeft(alien));
                Canvas.SetTop(alien_2, Canvas.GetTop(alien));
                Canvas.SetLeft(cosmo, Canvas.GetLeft(alien));
                Canvas.SetTop(cosmo, Canvas.GetTop(alien));
            }
        }

        private bool CollisionAvecBord(Canvas canvas, UIElement element)
        {

            double gauche = Canvas.GetLeft(element);
            double haut = Canvas.GetTop(element);
            double largeur = element.RenderSize.Width;
            double hauteur = element.RenderSize.Height;

            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            bool toucheGauche = gauche <= -largeur;
            bool toucheDroite = gauche >= canvasWidth;
            bool toucheHaut = haut <= -hauteur;
            bool toucheBas = haut >= canvasHeight;

            return toucheGauche || toucheDroite || toucheHaut || toucheBas;
        }

        private bool CollisionEntreEntité(UIElement element1, UIElement element2)
        {

            double gauche = Canvas.GetLeft(element1);
            double haut = Canvas.GetTop(element1);
            double largeur = element1.RenderSize.Width;
            double hauteur = element1.RenderSize.Height;

            double gauche2 = Canvas.GetLeft(element2);
            double haut2 = Canvas.GetTop(element2);
            double largeur2 = element2.RenderSize.Width;
            double hauteur2 = element2.RenderSize.Height;

            bool toucheGauche = gauche <= gauche2 + largeur2;
            bool toucheDroite = gauche + largeur >= gauche2;
            bool toucheHaut = haut <= haut2 + hauteur2;
            bool toucheBas = haut + hauteur >= haut2;

            return (toucheGauche && toucheDroite && toucheHaut && toucheBas);
        }
    }
 }