using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace subtitles
{
    public class Subtitle
    {
        public DateTime TimeStart;
        public DateTime TimeEnd;

        public string Title;

        public Location Location;
        public ConsoleColor Color;

        private Subtitle(DateTime timeStart, DateTime timeEnd, string subtitle, string location = "Bottom", string color = "White")
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;

            Title = subtitle;

            Location = Location.FindLocation(location);
            Color = Colour.GetColour(color);
        }

        public static StreamReader FindFile() => File.Exists("file.txt") ? new StreamReader("file.txt") : throw new Exception("file does not exist");


        public static Queue<Subtitle> ReadFile()
        {
            var queue = new Queue<Subtitle>();
            var file = FindFile();

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
            DateTime timeStart = Convert.ToDateTime(title[0..4]);
            DateTime timeEnd = Convert.ToDateTime(title[8..12]);

            if (title.Contains('['))
            {
                var info = title[(title.IndexOf('[') + 1)..(title.IndexOf(']') - 1)].Split(", ");
                var subtitle = title[(title.IndexOf(']') + 2)..];

                return new Subtitle(timeStart, timeEnd, subtitle, info[0], info[1]);
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
                var text = Queue.Peek();
                WaitUntil(text.TimeStart);
                CurrentText = text.Title;
                WaitUntil(text.TimeEnd);
                CurrentText = string.Empty;
                Queue.Dequeue();
            }
        }

        public void WaitUntil(DateTime time)
        {
            try
            {
                Thread.Sleep(time - DateTime.Now);
            }
            catch { }
        }

        public static void ArrangeQueue(Queue<Subtitle> queue)
        {
            var temp = queue.OrderBy(time => time.TimeStart);

            foreach(var item in temp)
            {
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

            var tasks = new Task[]
            {
                Task.Run(() => Location.Top.ProcessAllSubtitles()),
                Task.Run(() => Location.Bottom.ProcessAllSubtitles()),
                Task.Run(() => Location.Left.ProcessAllSubtitles()),
                Task.Run(() => Location.Right.ProcessAllSubtitles()),
            };

            while(!tasks.All(task => task.IsCompleted))
            {
                DrawFrame();
            }
        }

        private static void DrawFrame()
        {
            Console.WriteLine("╔" + new string('═', 74) + "╗");
            WriteTitle(Location.Top.CurrentText, Location.Top.Color, Position.Middle);

            DrawSides();
            WriteTitle(Location.Left.CurrentText, Location.Left.Color, Position.Left);
            WriteTitle(Location.Right.CurrentText, Location.Right.Color, Position.Right);
            DrawSides();

            WriteTitle(Location.Bottom.CurrentText, Location.Bottom.Color, Position.Middle);
            Console.WriteLine("╚" + new string('═', 74) + "╝");

        }

        private static void DrawSides()
        {
            for (int j = 1; j < 15; j++)
                Console.WriteLine("║" + new string(' ', 74) + "║");
        }

        private static void WriteTitle(string text, ConsoleColor color, Position side)
        {
            if(side == Position.Left || side == Position.Middle)
                Console.Write("║");
            
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();

            if (side == Position.Right || side == Position.Middle)
                Console.WriteLine("║");

        }
        private enum Position
        {
            Left,
            Right,
            Middle
        }
    }
}