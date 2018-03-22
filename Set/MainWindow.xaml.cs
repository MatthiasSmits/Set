﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Set
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        int score = 0;
        
        public List<Card> GenerateDeck()
        {
            List<Card> deck = new List<Card>();
            for (int n = 0; n < 3; n++)
            {
                for (int c = 0; c < 3; c++)
                {
                    for (int s = 0; s < 3; s++)
                    {
                        for (int f = 0; f < 3; f++)
                        {
                            Card card = new Set.Card(n, s, c, f);
                            deck.Add(card);
                        }
                    }
                }
            }
            return deck;
        }
        public List<Card> Deck;
        
        Random rng = new Random();
        List<Card> Shuffle(List<Card> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
        public void FillGrid(List<Card> visibleCards)
        {
            Card_Grid.Children.Clear();
            int columns = visibleCards.Count / 3;
            int cardnr = 0;
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    Card c = visibleCards[cardnr];
                    Grid.SetRow(c, row);
                    Grid.SetColumn(c, col);
                    Card_Grid.Children.Add(c);
                    cardnr++;
                }
            }
        }
        private void Check_Button_Click(object sender, RoutedEventArgs e)
        {
            if (IsSet(selectedCards))
            {
                score++;
                ScoreBlock.Text = string.Format("Score: {0}", score);
                if (Deck.Count == 0)
                {
                    MessageBox.Show("Winner winner chicken dinner!");
                }
                else
                {
                    foreach (Card c in selectedCards)
                    {
                        VisibleCards.Remove(c);
                    }
                    selectedCards.Clear();
                    while (VisibleCards.Count < 12)
                    {
                        DrawCard();
                    }
                    FillGrid(VisibleCards);
                    Check_Button.IsEnabled = false;
                    Add_Col.IsEnabled = true;
                }
            }
            else
            {
                score--;
                ScoreBlock.Text = string.Format("Score: {0}", score);
                foreach(Card c in selectedCards)
                {
                    c.BorderBrush = Brushes.Black;
                }
                selectedCards.Clear();
                Check_Button.IsEnabled = false;
            }
        }

        List<Card> VisibleCards = new List<Card>();
        public MainWindow()
        {
            InitializeComponent();
            Deck = GenerateDeck();
            Deck = Shuffle(Deck);
            for (int i = 0; i < 12; i++)
            {
                DrawCard();
            }
            FillGrid(VisibleCards);
        }
        public void DrawCard()
        {
            Card c = Deck[0];
            c.Click += Card_Select;
            VisibleCards.Add(c);
            Deck.Remove(c);
        }
        public List<Card> selectedCards = new List<Card>();
        private void Card_Select(object sender, RoutedEventArgs e)
        {
            Card c = sender as Card;
            if (!selectedCards.Contains(c))
            {
                if (selectedCards.Count == 3)
                {
                    Card removeCard = selectedCards[0];
                    selectedCards.Remove(removeCard);
                    removeCard.BorderBrush = Brushes.Black;
                }
                selectedCards.Add(c);
                c.BorderBrush = Brushes.Red;
            }
            else
            {
                selectedCards.Remove(c);
                c.BorderBrush = Brushes.Black;
            }
            Check_Button.IsEnabled = selectedCards.Count == 3;
        }
        public bool IsSet(List<Card> selection)
        {
            bool isSet = true;
            for (int i = 0; i < 4; i++)
            {
                int a = selection[0].Prop[i];
                int b = selection[1].Prop[i];
                int c = selection[2].Prop[i];

                if (a == b & b != c) //two the same, but the third is not the same: not a set (AAB)
                {
                    isSet = false;
                }
                if ((a != b & a != c) & b == c) // two are different and the third is different in the same way (ABB)
                {
                    isSet = false;
                }
            }
            return isSet;
        }

        private void Add_Col_Click(object sender, RoutedEventArgs e)
        {
            Add_Col.IsEnabled = false;
            for (int i = 0; i < 3; i++)
            {
                DrawCard();
            }
            FillGrid(VisibleCards);
            
        }
    }
    public class Card : Button
    {
        public int Number;
        public string Shape;
        public Color Color;
        public double Fill;
        public int[] Prop;
        private int[] numbers = { 1, 2, 3 };
        private string[] shapes = { "oval", "diamond", "rectangle" };
        private Color[] colors = { Colors.Purple, Colors.Green, Colors.Red };
        private double[] fills = { 0, 0.3, 1 };
        public Card(int n, int s, int c, int f) : base()
        {
            Number = numbers[n];
            Shape = shapes[s];
            Color = colors[c];
            Fill = fills[f];
            Prop = new int[] { n, s, c, f};
            Content = CardVisual();
            Background = Brushes.White;
            BorderBrush = Brushes.Black;
        }
        public Grid CardVisual()
        {
            Grid g = new Grid();
            
            for (int i = 0; i < Number; i++)
            {
                g.RowDefinitions.Add(new RowDefinition());
                Shape s = symbol();
                s.Fill = fillBrush();
                s.Stroke = strokeBrush();
                Grid.SetRow(s, i);
                g.Children.Add(s);
            }
            return g;
        }
        private Shape symbol()
        {
            switch (Shape)
            {
                case "oval":
                    Ellipse e = new Ellipse {Width = 70, Height = 40, Margin = new Thickness(5) };
                    return e;
                case "diamond":
                    Point p1 = new Point(0,20);
                    Point p2 = new Point(35, 40);
                    Point p3 = new Point(70, 20);
                    Point p4 = new Point(35, 0);
                    PointCollection pc = new PointCollection() {p1, p2, p3, p4 };
                    Polygon d = new Polygon { Points = pc, Width = 70, Height = 40, Margin = new Thickness(5) };
                    return d;
                case "rectangle":
                    Rectangle r = new Rectangle { Width = 70, Height = 40, Margin = new Thickness(5) };
                    return r;
                default:
                    return null;

            }
        }
        private SolidColorBrush fillBrush()
        {
            SolidColorBrush b = new SolidColorBrush();
            b.Color = Color;
            b.Opacity = Fill;
            return b;
        }
        private SolidColorBrush strokeBrush()
        {
            SolidColorBrush b = new SolidColorBrush();
            b.Color = Color;
            return b;
        }
    }
}
