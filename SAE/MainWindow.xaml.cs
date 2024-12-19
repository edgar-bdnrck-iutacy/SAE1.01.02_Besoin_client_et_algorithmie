using Microsoft.VisualBasic;
using System;
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
using System.Threading;
using static System.Formats.Asn1.AsnWriter;
using System.Windows.Media.Animation;
using System.Security.Cryptography.X509Certificates;
using System.Data;

namespace SAE
{
    public partial class MainWindow : Window
    {
        public static readonly int VITESSE_ALIEN2 = 4, DEMITOUR = 180,AJUSTEMENT_ANGLE = 90, HORS_ECRAN_X = -100, HORS_ECRAN_Y = -100, SCOREMAX4 = 50, SCOREMAX1 = 10, SCOREMAX2 = 15, LIMITE_DROITE_ALIEN = 800, POSITION_ALIEN2_LEFT = 50, POSITION_ALIEN2_TOP = 200, POSITION_ALIEN_TOP = 280, POSITION_ALIEN_LEFT = 360, FPS = 16, LIMITE_GAUCHE_ALIEN = 20, SCOREMAX3 = 25, VITESSE_LAZER = 15, GRANDEVITESSE = 30, AJUSTEMENTVOLUME = 10;
        public static readonly double ACCELERATION_PAR_TICK = 0.25, ACCELERATION_DECOLAGE = 0.5;

        public bool niv1fini = false, niv2fini = false, niv3fini = false, invinsible =  false, dejaAppele = false, dejaAppele2 = false, versDroite = true, decolage = false, lobby = true, pause = false, interaction = false, gauche = false, droite = false, haut = false, bas = false, lazerTire = false;
        private static DispatcherTimer tick;
        private static double vitesseFusee = 0,distanceX = 0, distanceY = 0, vitesse = 2, vitessemax = 10, vitesseAlien = 5, trajectoireX, trajectoireY, trajectoireX_2, trajectoireY_2;
        private static int score = 0, niveau = 0, scoreMax = 0, nbNiveauComplete = 0, tempsRestantInvisibilite = 0;
        private MediaPlayer musique;
        private Random random = new Random();


        public MainWindow()
        {
            InitializeComponent();
            LanceMenuDemarrage();
            InitialiseLobby();
            this.MouseMove += DeplacementSouris;
            

            //Init du lecteur de média pour la musique de niveaux
            musique = new MediaPlayer();
            musique.Open(new Uri("music/Level.mp3", UriKind.Relative));

            //Definit Volume par rapport au slider vu en paramètres
            musique.Volume = Parametre.Volume / AJUSTEMENTVOLUME;

            //Lecutre en boucle
            musique.MediaEnded += (s, e) =>
            {
                musique.Position = TimeSpan.Zero;
                musique.Play();
            };

            musique.Play();

            // Changement du volume en temps réel
            Parametre.changementVolume += MajVolume;
            InitTimer();
        }

        private void LanceMenuDemarrage()
        {
            this.Hide();
            MenuDemarrage dialog = new MenuDemarrage();
            bool? result = dialog.ShowDialog();
            if (result == false)
                Application.Current.Shutdown();
            this.Show();
        }

        private void MajVolume(double volume)
        {
            musique.Volume = volume / AJUSTEMENTVOLUME; // Mise a jour du volume
            Console.WriteLine($"Volume mis a jour dans le MainWindow: {musique.Volume}");
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            // Appel des touches depuis les parametres
            if (e.Key.ToString() == Parametre.KeyAvancer)
                haut = false;
            else if (e.Key.ToString() == Parametre.KeyReculer)
                bas = false;
            else if (e.Key.ToString() == Parametre.KeyGauche)
                gauche = false;
            else if (e.Key.ToString() == Parametre.KeyDroite)
                droite = false;

            switch (e.Key)
            {
                case Key.Space:
                    interaction = false;
                    break;
            }
        }

        private void InitTimer()
        {
            tick = new DispatcherTimer();
            tick.Interval = TimeSpan.FromMilliseconds(FPS);
            tick.Tick += Jeu;
            tick.Start();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.Key);

            // Appel des touches depuis les parametres
            if (e.Key.ToString() == Parametre.KeyAvancer)
                haut = true;
            else if (e.Key.ToString() == Parametre.KeyReculer)
                bas = true;
            else if (e.Key.ToString() == Parametre.KeyGauche)
                gauche = true;
            else if (e.Key.ToString() == Parametre.KeyDroite)
                droite = true;
            else if (e.Key.ToString() == Parametre.KeyPause)
                pause = !pause;
            else if (e.Key.ToString() == Parametre.KeyAvanceAuto)
                haut = !haut;

            switch (e.Key)
            {
                case Key.Space:
                    interaction = true;
                    break;

                // code de triche
                case Key.O:
                    score++;
                    break;
                case Key.I:
                    vitessemax = GRANDEVITESSE;
                    break;

            }
        }

        private void Jeu(object sender, EventArgs e)
        {
            labelScore.Content = $"{score}/{scoreMax}";
            bool enMouvement = (droite || gauche || haut || bas) && !(droite && gauche) && !(haut && bas);
            if (!pause)
            {
                if (!dejaAppele2 && !interaction && niveau == 0)
                {
                    rapportSpacial1.Visibility = Visibility.Visible;
                }
                else if (!dejaAppele2 && !interaction && nbNiveauComplete < 3)
                {
                    rapportSpacial2.Visibility = Visibility.Visible;
                }
                else if (!dejaAppele2 && !interaction && nbNiveauComplete == 3)
                {
                    rapportSpacial4.Visibility = Visibility.Visible;

                }
                else
                {
                    rapportSpacial4.Visibility = Visibility.Hidden;
                    rapportSpacial1.Visibility = Visibility.Hidden;
                    rapportSpacial2.Visibility = Visibility.Hidden;
                    dejaAppele2 = true;

                    labelPause.Visibility = Visibility.Hidden;
                    Decolage();
                    DeplacementCosmo();
                }
                if (enMouvement)
                {
                    if (vitesse < vitessemax)
                    {
                        vitesse = vitesse + ACCELERATION_PAR_TICK;
                    }
                }
                else
                {
                    vitesse = 0;
                }

                if (!lobby)
                {

                    if (tempsRestantInvisibilite  == 0)
                    {
                        invinsible = false;
                    } 
                    else
                    {
                        tempsRestantInvisibilite--;
                    }

                    if (!dejaAppele)
                    {
                        InitialiseNiv();
                    }

                    if (score == scoreMax)
                    {
                        switch (niveau)
                        {
                            case 1:
                                if (!niv1fini)
                                    nbNiveauComplete++;
                                break;
                            case 2:
                                if (!niv2fini)
                                    nbNiveauComplete++;
                                break;
                            case 3:
                                if (!niv3fini)
                                    nbNiveauComplete++;
                                break;
                        }
                        score = 0;
                        lobby = true;
                        Fusee.Source = new BitmapImage(new Uri($"img/fuseeStage{nbNiveauComplete + 1}.png", UriKind.Relative));
                        debris.Source = new BitmapImage(new Uri($"img/debrisStage{nbNiveauComplete + 1}.png", UriKind.Relative));
                        dejaAppele = false;
                        dejaAppele2 = false;
                        switch (niveau)
                        {
                            case 1:
                            {
                                niv1fini = true;
                                break;
                            }
                            case 2:
                            {
                                niv2fini = true;
                                break;
                            }
                            case 3:
                            {
                                niv3fini = true;
                                break;
                            }

                        }
                    }

                    if (CollisionEntreEntite(cosmo, satellite))
                    {
                        score++;
                        labelScore.Content = $"{score}/{scoreMax}";
                        do
                        {
                            Canvas.SetLeft(satellite, random.Next(0, (int)(canvas.ActualWidth - satellite.Width)));
                            Canvas.SetTop(satellite, random.Next(0, (int)(canvas.ActualHeight - satellite.Height)));
                        }
                        while (CollisionEntreEntite(alien, satellite));
                    }

                    switch (niveau)
                    {
                        case 2:
                            DeplacementAlien_2GaucheDroite();
                            break;
                        case 3:
                            Canvas.SetLeft(lazer_2, Canvas.GetLeft(alien));
                            Canvas.SetTop(lazer_2, Canvas.GetLeft(alien));
                            InitialiseTrajectoireAlienRouge();
                            DeplacementAlien();
                            break;
                    }

                    if (!lazerTire)
                    {
                        InitialiseTrajectoireLazer(lazer, alien , ref trajectoireX, ref trajectoireY);
                        if (niveau >= 2)
                            InitialiseTrajectoireLazer(lazer_2, alien_2, ref trajectoireX_2, ref trajectoireY_2);
                    }
                    else
                    {
                        TirLazer(lazer, trajectoireX, trajectoireY);
                        if (niveau >= 2)
                            TirLazer(lazer_2, trajectoireX_2, trajectoireY_2);
                    }

                    if (!invinsible)
                    {
                        if (CollisionMortel())
                        {
                            LabelGameOver.Visibility = Visibility.Visible;
                            lobby = true;
                            score = 0;
                            dejaAppele = false;
                            Canvas.SetLeft(cosmo, canvas.ActualWidth / 2);
                            Canvas.SetTop(cosmo, canvas.ActualHeight / 2);

                        }
                        cosmo.Opacity = 1;
                    }
                    else
                    {
                        cosmo.Opacity = 0.5;
                        LabelGameOver.Visibility = Visibility.Hidden;
                    }
                }
                else
                {
                    InitialiseLobby();
                    if (CollisionEntreEntite(cosmo, Fusee))
                    {
                        if (interaction) 
                        {
                            if (nbNiveauComplete == 3)
                            {
                                FinJeu();
                                niveau = 0;
                                decolage = true;

                            }
                            else
                            {
                                SelecteurNiveau();
                            }
                        }
                    }
                }
            }
            else
            {
                labelPause.Visibility = Visibility.Visible;
            }
        }

        private void DeplacementAlien_2GaucheDroite() 
        {
            if (versDroite)
            {
                Canvas.SetLeft(alien_2, Canvas.GetLeft(alien_2) + vitesseAlien);
                if (Canvas.GetLeft(alien_2) > LIMITE_DROITE_ALIEN)
                {
                    versDroite = false;
                }
            }
            else
            {
                Canvas.SetLeft(alien_2, Canvas.GetLeft(alien_2) - vitesseAlien);
                if (Canvas.GetLeft(alien_2) < LIMITE_GAUCHE_ALIEN)
                {
                    versDroite = true;
                }
            }
        }

        private void SelecteurNiveau() 
        {
            this.Hide();
            SelecteurNiveau dialog = new SelecteurNiveau();
            bool? result = dialog.ShowDialog();
            if (dialog.niveauSelectionne > 0)
            {
                lobby = false;
                niveau = dialog.niveauSelectionne;
            }
            this.Show();
            interaction = false;
            invinsible = true;
            tempsRestantInvisibilite = 50;
        }

        private bool CollisionMortel()
        {
            if (CollisionEntreEntite(lazer, cosmo) || CollisionEntreEntite(lazer_2, cosmo) || CollisionEntreEntite(alien, cosmo) || CollisionEntreEntite(alien_2, cosmo))
            {
                return true;
            } 
            else
            {
                return false;
            }
        } 

        private void Decolage()
        {
            if (decolage)
            {
                if (Canvas.GetTop(Fusee) > -Fusee.Height)
                {
                    vitesseFusee = vitesseFusee + ACCELERATION_DECOLAGE;
                    Canvas.SetTop(Fusee, Canvas.GetTop(Fusee) - vitesseFusee);
                }
                else 
                {
                    decolage = false;
                    MenuDemarrage menuDemarrage = new MenuDemarrage();
                    menuDemarrage.ShowDialog();
                    //ReinitialiseJeu();
                }
            }
        }

        private void FinJeu()
        {
            dejaAppele = true;
            interaction = false;

            cosmo.Visibility = Visibility.Hidden;
            Fusee.Source = new BitmapImage(new Uri($"img/fuseeDepart.png", UriKind.Relative));
            vitesseFusee = 0;
        }

        private void InitialiseNiv()
        {
            SolLunaire.Visibility = Visibility.Hidden;
            Fusee.Visibility = Visibility.Hidden;
            debris.Visibility = Visibility.Hidden;

            labelScore.Visibility = Visibility.Visible;
            satellite.Visibility = Visibility.Visible;
            alien.Visibility = Visibility.Visible;
            lazer.Visibility = Visibility.Visible;
            meteorite1.Visibility = Visibility.Visible;
            meteorite2.Visibility = Visibility.Visible;
            meteorite3.Visibility = Visibility.Visible;
            dejaAppele = true;

            switch (niveau)
            {
                case 1 : 
                    {
                        scoreMax = SCOREMAX1;
                        Canvas.SetLeft(alien, POSITION_ALIEN_LEFT);
                        Canvas.SetTop(alien, POSITION_ALIEN_TOP);
                        Canvas.SetLeft(alien_2, HORS_ECRAN_X);
                        Canvas.SetTop(alien_2, HORS_ECRAN_Y);
                        break;
                    }

                case 2:
                    {
                        lazer_2.Visibility = Visibility.Visible;
                        alien_2.Visibility = Visibility.Visible;
                        scoreMax = SCOREMAX2;
                        Canvas.SetLeft(alien, POSITION_ALIEN_LEFT);
                        Canvas.SetTop(alien, POSITION_ALIEN_TOP);
                        Canvas.SetLeft(alien_2, POSITION_ALIEN2_LEFT);
                        Canvas.SetTop(alien_2, POSITION_ALIEN2_TOP);
                        break;
                    }

                case 3:
                    {
                        Canvas.SetLeft(alien, POSITION_ALIEN_LEFT);
                        Canvas.SetTop(alien, POSITION_ALIEN_TOP);
                        Canvas.SetLeft(alien_2, POSITION_ALIEN2_LEFT);
                        Canvas.SetTop(alien_2, POSITION_ALIEN2_TOP);
                        alien_2.Visibility = Visibility.Visible;
                        scoreMax = SCOREMAX3;
                        break;
                    }
                case 4:
                    {
                        scoreMax = SCOREMAX4;
                        break;
                    }

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
            meteorite1.Visibility = Visibility.Hidden;
            meteorite2.Visibility = Visibility.Hidden;
            meteorite3.Visibility = Visibility.Hidden;

            SolLunaire.Visibility = Visibility.Visible;
            Fusee.Visibility = Visibility.Visible;
            debris.Visibility = Visibility.Visible;
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
                    double angle = Math.Atan2(distanceY, distanceX) * (DEMITOUR / Math.PI) + (DEMITOUR/2); // Conversion en degrés et ajustement de l'angle

                    cosmo.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

                    // Créer un objet RotateTransform avec l'angle calculé et le centre comme origine de la rotation
                    RotateTransform Rotation = new RotateTransform(angle);

                    // Appliquer la transformation de rotation a cosmo
                    cosmo.RenderTransform = Rotation;
            }
        }

        private void InitialiseTrajectoireLazer(Image lazer, Image alien, ref double trajectoireX, ref double trajectoireY)
        {
            Canvas.SetLeft(lazer, Canvas.GetLeft(alien) + alien.Width / 2);
            Canvas.SetTop(lazer, Canvas.GetTop(alien) + alien.Height / 2);
            lazerTire = true;

            distanceX = (Canvas.GetLeft(cosmo) + cosmo.Width / 2) - (Canvas.GetLeft(lazer) + lazer.Width / 2);
            distanceY = (Canvas.GetTop(cosmo) + cosmo.Height / 2) - (Canvas.GetTop(lazer) + lazer.Height / 2);

            // Calcule l'angle en utilisant la fonction Atan2
            double angle = Math.Atan2(distanceY, distanceX) * (DEMITOUR / Math.PI) + DEMITOUR; // Conversion en degrés et ajustement de l'angle

            lazer.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);

            // Créer un objet RotateTransform avec l'angle calculé et le centre comme origine de la rotation
            RotateTransform Rotation = new RotateTransform(angle);

            // Appliquer la transformation de rotation a cosmo
            lazer.RenderTransform = Rotation;

            double distanceXY = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);

            trajectoireX = distanceX / distanceXY;
            trajectoireY = distanceY / distanceXY;
        }

        private void InitialiseTrajectoireAlienRouge()
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
                    double angleRadians = angle * Math.PI / DEMITOUR;

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
            if (haut && bas)
            {
                Console.WriteLine("haut et bas");
            }
            else if (haut)
            {
                // Récupérer la position actuelle de la souris relative au Canvas
                System.Windows.Point PositionSouris = Mouse.GetPosition(this);
                if(!(PositionSouris.X < 10 && PositionSouris.Y < 10))
                {
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

        private void TirLazer(Image lazer, double trajectoireX, double trajectoireY)
        {
            Canvas.SetLeft(lazer, Canvas.GetLeft(lazer) + trajectoireX * VITESSE_LAZER);
            Canvas.SetTop(lazer, Canvas.GetTop(lazer) + trajectoireY * VITESSE_LAZER);


            if (CollisionEntreEntite(lazer, cosmo))
            {
                lazerTire = false;
            } 
            if (CollisionAvecBord(canvas,lazer)) 
            {
                lazerTire = false;
            }
        }

        private void DeplacementAlien()
        {
            Canvas.SetLeft(alien_2, Canvas.GetLeft(alien_2) + trajectoireX_2 * VITESSE_ALIEN2);
            Canvas.SetTop(alien_2, Canvas.GetTop(alien_2) + trajectoireY_2 * VITESSE_ALIEN2);

            if (CollisionEntreEntite(cosmo, alien_2))
            {
                Canvas.SetLeft(alien_2, Canvas.GetLeft(alien));
                Canvas.SetTop(alien_2, Canvas.GetTop(alien));
                Canvas.SetLeft(cosmo, Canvas.GetLeft(alien));
                Canvas.SetTop(cosmo, Canvas.GetTop(alien));
            }
        }

        private bool CollisionAvecBord(Canvas canvas, Image element)
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

        private bool CollisionEntreEntite(Image element1, Image element2)
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