using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace subtitles
{
    public class Subtitle
    {
        public DateTime TimeStart;
        public DateTime TimeEnd;
        public TimeSpan Duration; //продолжительность
        //TimeSpan это буквально структура показывающая сколько времени прошло между двумя точками во времени. Если вычитать DateTime то оно будет TimeSpan.

        public static Location Location;
        public ConsoleColor Color;
        
        public string Title;

        public Subtitle(DateTime timeStart, DateTime timeEnd, string location, string color, string subtitle)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            Duration = TimeEnd - TimeStart;

            Location = Location.FindLocation(location);
            Color = Colour.GetColour(color);

            Title = subtitle;
        }

        public Subtitle(DateTime timeStart, DateTime timeEnd, string subtitle)
        {
            TimeStart = timeStart;
            TimeEnd = timeEnd;
            Duration = TimeEnd - TimeStart;
            
            Title = subtitle;
        }

        public void ArrangeQueue()
        {
            var queue = ReadFile();

            queue = (Queue<Subtitle>)queue.OrderBy(time => time.TimeStart); //((x, y) => x.CompareTo(y)));

            while(queue.Count > 0)
            {
                Location.Queue.Enqueue(queue.Dequeue());
            }
        }

        public static Queue<Subtitle> ReadFile()
        {
            var queue = new Queue<Subtitle>();
            var file = new StreamReader("file.txt");

            while(!file.EndOfStream)
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
        public Queue<Subtitle> Queue = new Queue<Subtitle>();
        public string Str;

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
            DrawFrame();
        }

        public static void DrawFrame()
        {
            Console.Write("╔");
            DrawPart("═", 50);
            Console.WriteLine("╗");
            
            for(int j = 1; j < 22; j++)
            {
                Console.Write("║");
                DrawPart(" ", 50);
                Console.WriteLine("║");
            }

            Console.Write("╚");
            DrawPart("═", 50);
            Console.Write("╝");
        }

        public static void DrawPart(string symbol, int num)
        {
            for (int i = 0; i <= num; i++)
            {
                Console.Write(symbol);
            }
        }

    }
}
