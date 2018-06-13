using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public Deck deck;
        public Table table;
        public MainWindow()
        {
            InitializeComponent();
            deck = new Set.Deck(Card_Select);
            table = new Set.Table(deck);
            Debug.Assert(table.OpenCards.Count == 12);
            Card_Grid = Table.FillGrid(Card_Grid, table.OpenCards);
            Type test = Card_Grid.Children[0].GetType();
            Debug.Assert(test.Equals(typeof(Card)));
        }
        private void Card_Select(object sender, RoutedEventArgs e)
        {
            Card c = sender as Card;
            table.SelectCard(c);
            Check_Button.IsEnabled = table.SelectedThree;
        }
        private void Add_Col_Click(object sender, RoutedEventArgs e)
        {
            Add_Col.IsEnabled = false;
            table.Draw(3);
            Card_Grid = Table.FillGrid(Card_Grid, table.OpenCards);
            
        }
        private void Check_Button_Click(object sender, RoutedEventArgs e)
        {
            table.CheckSet();
            ScoreBlock.Text = string.Format("Score: {0}", table.Score);
            Check_Button.IsEnabled = false;
            Add_Col.IsEnabled = true;
            if (deck.IsEmpty)
            {
                MessageBox.Show("Winner winner chicken dinner!");
            }
            else
            {
                Card_Grid = Table.FillGrid( Card_Grid, table.OpenCards);
            }
        }
    }
    public class Deck
    {
        private List<Card> decklist;
        private RoutedEventHandler cardClick;
        public Deck(RoutedEventHandler c)
        {
            decklist = Deck.Generate();
            decklist = Shuffle(decklist);
            cardClick = c;
            Debug.Assert(decklist.Count == 3*3*3*3);
            Debug.WriteLine("In Deck constructor, generated Cards.");
        }
        public static List<Card> Generate()
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
        private static Random rng = new Random();
        public static List<Card> Shuffle(List<Card> list)
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
        public Card Draw()
        {
            Card c = decklist[0];
            decklist.Remove(c);
            c.Click += cardClick;
            return c;
        }
        public bool IsEmpty
        {
            get { return decklist.Count == 0; }
        }
    }
    public class Table
    {
        private Deck openDeck;
        public Table(Deck deck)
        {
            selectedCards = new List<Card>();
            OpenCards = new List<Card>();
            openDeck = deck;
            Draw(12);
            Debug.Assert(OpenCards.Count == 12);
            Debug.WriteLine("In Table constructor, received Cards.");
            score = 0;
        }
        public List<Card> OpenCards;
        public void Draw(int n)
        {
            for (int i = 0; i < n; i++)
            {
                OpenCards.Add(openDeck.Draw());
            }
        }
        private List<Card> selectedCards;
        public void SelectCard(Card card)
        {
            if (!selectedCards.Contains(card))
            {
                if (this.SelectedThree)
                {
                    Card removeCard = selectedCards[0];
                    selectedCards.Remove(removeCard);
                    removeCard.BorderBrush = Brushes.Black;
                }
                selectedCards.Add(card);
                card.BorderBrush = Brushes.Red;
            }
            else
            {
                selectedCards.Remove(card);
                card.BorderBrush = Brushes.Black;
            }
        }
        public bool SelectedThree { get {return selectedCards.Count == 3; } }
        public int Score { get { return score; } }
        private int score;
        public bool CheckSet()
        {
            if (Card.IsSet(selectedCards))
            {
                score++;
                if (openDeck.IsEmpty)
                {
                    MessageBox.Show("Winner winner chicken dinner!");
                }
                else
                {
                    foreach (Card c in selectedCards)
                    {
                        OpenCards.Remove(c);
                    }
                    selectedCards.Clear();
                    this.Draw(12 - OpenCards.Count);
                }
                return true;
            }
            else
            {
                score--;
                foreach (Card c in selectedCards)
                {
                    c.BorderBrush = Brushes.Black;
                }
                selectedCards.Clear();
                return false;
            }

        }
        public static Grid FillGrid(Grid g, List<Card> visibleCards)
        {
            Debug.Assert(visibleCards.Count > 1);
            Debug.WriteLine("In Table.FillGrid(), received Cards.");
            g.Children.Clear();
            int columns = visibleCards.Count / 3;
            int cardnr = 0;
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < 3; row++)
                {
                    Card c = visibleCards[cardnr];
                    Grid.SetRow(c, row);
                    Grid.SetColumn(c, col);
                    g.Children.Add(c);
                    cardnr++;
                }
            }
            return g;
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
        private string[] shapes = { "oval", "diamond", "squiggle" };
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
        public Canvas CardVisual()
        {
            Canvas canvas = new Canvas();
            
            for (int i = 0; i < Number; i++)
            {
                Shape s = symbol();
                s.Fill = fillBrush();
                s.Stroke = strokeBrush();
                int topOffset = 0 - 25*Number + 50*i;
                Canvas.SetTop(s, topOffset);
                Canvas.SetLeft(s, -40);
                canvas.Children.Add(s);
            }
            return canvas;
        }
        public static bool IsSet(List<Card> selection)
        {
            if (selection.Count == 3)
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
            else return false;
        }
        private Shape symbol()
        {
            switch (Shape)
            {
                case "oval":
                    Rectangle e = new Rectangle { Width = 70, Height = 40, RadiusX = 20, RadiusY = 20, Margin = new Thickness(5), StrokeThickness = 2 };
                    return e;
                case "diamond":
                    Point p1 = new Point(0,20);
                    Point p2 = new Point(35, 40);
                    Point p3 = new Point(70, 20);
                    Point p4 = new Point(35, 0);
                    PointCollection pc = new PointCollection() {p1, p2, p3, p4 };
                    Polygon d = new Polygon { Points = pc, Width = 70, Height = 40, Margin = new Thickness(5), StrokeThickness = 2 };
                    return d;
                case "squiggle":
                    string path = "M 0,27 c -1,24 7,-13 32,2.5 24,13 36,5 36,-20 1,-24 -8,9 -32,-5.75 -24,-14.75 -36,0.25 -36,23.25 z";
                    Geometry g = Geometry.Parse(path);
                    Path p = new Path { Data = g, Height = 40, Width = 70, Margin = new Thickness(5), StrokeThickness = 2 };
                    return p;
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
