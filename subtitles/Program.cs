using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace subtitles
{
    public class Subtitle
    {
        public DateTime TimeStart;
        public DateTime TimeEnd;

        public Location Location;
        public ConsoleColor Color;

        public string Title;

        private Subtitle(DateTime timeStart, DateTime timeEnd, string location, string color, string subtitle)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;

            Location = Location.FindLocation(location);
            Color = Colour.GetColour(color);

            Title = subtitle;
        }

        private Subtitle(DateTime timeStart, DateTime timeEnd, string subtitle)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;

            Title = subtitle;
        }

        public static Queue<Subtitle> ReadFile()
        {
            var queue = new Queue<Subtitle>();
            var file = new StreamReader("file.txt");

            while (!file.EndOfStream)
            {
                queue.Enqueue(ReadSubtitle(file.ReadLine()));
            }

            return queue;
        }

        //00:01 - 00:03 [Top, Red] Hello
        //00:07 - 00:15 Bill is a very motivated young man
        public static Subtitle ReadSubtitle(string title)
        {
            DateTime timeStart = Convert.ToDateTime(title.Substring(0, 4));
            DateTime timeEnd = Convert.ToDateTime(title.Substring(8, 12));

            if (title.Contains('['))
            {
                var info = title[(title.IndexOf('[') + 1)..(title.IndexOf(']') - 1)].Split(", ");
                var subtitle = title[(title.IndexOf(']') + 2)..];

                return new Subtitle(timeStart, timeEnd, info[0], info[1], subtitle);
            }
            else
                return new Subtitle(timeStart, timeEnd, title[14..]);
        }

    }


    public class Location
    {
        public static Location Top = new Location();
        public static Location Left = new Location();
        public static Location Bottom = new Location();
        public static Location Right = new Location();
        private Queue<Subtitle> Queue = new Queue<Subtitle>();
        public ConsoleColor Color { get => Queue.Peek().Color; }
        
        private string currentText; //хранилище св-ва ниже (привет с маленькой буквы)
        public string CurrentText 
        {
            get => currentText; //гет вызывается когда запрашивается значение. данные нужны нечасто, но часто обрабатываются. (Числа добавляют постоянно а значение нужно редко)

            private set //Сет вызывается когда ты присваиваешь значение. данные запрашиваются часто но редко обрабатываются. (Числа добавляют редко, а значение запрашивают часто)
            {
                if (value.Length > 35) //value - и есть то присвоенное значение, из-за которого вазвался сет
                    currentText = value.Substring(0, 35);
                else
                {
                    if (this == Right) //является ли ссылка на самого себя ссылкой на Right
                        currentText = value.PadLeft(35);
                    else
                        currentText = value.PadRight(35);
                }
            }
        }

        public void ProcessAllSubtitles()
        {
            while(Queue.Count > 0)
            {
                var text = Queue.Dequeue();
                WaitUntil(text.TimeStart);
                CurrentText = text.Title;
                WaitUntil(text.TimeEnd);
                CurrentText = string.Empty;
            }
        }

        public void WaitUntil(DateTime time) => Thread.Sleep(time - DateTime.Now);

        public static void ArrangeQueue(Queue<Subtitle> queue)
        {
            queue = (Queue<Subtitle>)queue.OrderBy(time => time.TimeStart);

            for (int i = 0; i < queue.Count; i++)
            {
                var item = queue.Dequeue();
                item.Location.Queue.Enqueue(item);
            }
        }

        public static Location FindLocation(string str) => str switch
        {
            "Top" => Top,
            "Left" => Left,
            "Bottom" => Bottom,
            "Right" => Right,
            _ => Bottom,
        };

    }

    public static class Colour
    {
        public static ConsoleColor GetColour(string colour) => colour switch
        {
            "White" => ConsoleColor.White,
            "Red" => ConsoleColor.Red,
            "Blue" => ConsoleColor.Blue,
            "Green" => ConsoleColor.Green,
            "Yellow" => ConsoleColor.Yellow,
            "Black" => ConsoleColor.Black,
            _ => ConsoleColor.White,
        };
    }

    class Program
    {
        static void Main(string[] args)
        {
            Location.ArrangeQueue(Subtitle.ReadFile());
            DrawFrame();
        }

        public static void DrawFrame()
        {
            DrawCentralSide('╔', Location.Top.CurrentText, Location.Top.Color, '╗');
            
            DrawSides();
            WriteTitle(Location.Left.CurrentText, Location.Left.Color);
            WriteTitle(Location.Right.CurrentText, Location.Right.Color);
            DrawSides();

            DrawCentralSide('╚', Location.Bottom.CurrentText, Location.Bottom.Color, '╝');
        }
        
        public static void DrawCentralSide(char sym1, string text, ConsoleColor color, char sym2)
        {
            Console.Write(sym1);
            Console.Write(new string('═', 8));
            WriteTitle(text, color);
            Console.Write(new string('═', 8));
            Console.WriteLine(sym2);
        }

        public static void DrawSides()
        {
            for (int j = 1; j < 10; j++)
            {
                Console.Write('║');
                Console.Write(new string(' ', 50));
                Console.WriteLine('║');
            }
        }

        public static void WriteTitle(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

    }
}