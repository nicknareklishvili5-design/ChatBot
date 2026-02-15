using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace BotApplication
{
    public class Bot
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public Bot(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public virtual void Introduce() => Console.WriteLine($"Hello! I am {Name}, running on version {Version}.");
    }

    public class ChatBot : Bot
    {
        public string Language { get; set; }
        private List<string> _history = new List<string>();

        public string this[int index]
        {
            get => index >= 0 && index < _history.Count ? _history[index] : "No history.";
            set => _history.Add(value);
        }

        public ChatBot(string name, string version, string language) : base(name, version) { Language = language; }

        public override void Introduce() => Console.WriteLine($"[ChatBot] {Name} is online. (Type 'back' to exit)");

        public void Chat(string userInput)
        {
            this[_history.Count] = userInput;
            string response = userInput.ToLower() switch
            {
                var s when s.Contains("hello") || s.Contains("hi") => "Greetings! Ready to talk code or coffee?",
                var s when s.Contains("how are you") => "I'm functioning at 100% capacity, thank you!",
                var s when s.Contains("history") => $"Log: {string.Join(" -> ", _history)}",
                _ => "That's fascinating. Tell me more!"
            };
            Console.WriteLine($"[{Name}]: {response}");
        }
    }

    // --- WeatherBot (Live Data) ---
    public class WeatherBot : Bot
    {
        public string Region { get; set; }
        private static readonly HttpClient client = new HttpClient();
        public WeatherBot(string name, string version, string region) : base(name, version) { Region = region; }

        public async Task GetLiveWeatherAsync()
        {
            try
            {
                string url = "https://api.open-meteo.com/v1/forecast?latitude=40.55&longitude=-74.28&current_weather=true&temperature_unit=fahrenheit";
                string response = await client.GetStringAsync(url);
                using JsonDocument doc = JsonDocument.Parse(response);
                JsonElement current = doc.RootElement.GetProperty("current_weather");
                double temp = current.GetProperty("temperature").GetDouble();
                Console.WriteLine($"\n--- LIVE WEATHER: {Region} ---\nTemperature: {temp}°F\n-------------------------------");
            }
            catch { Console.WriteLine("Weather service currently unavailable."); }
        }
    }

    // --- TravelBot Class ---
    public class TravelBot : Bot
    {
        private Dictionary<string, string> _destinations = new Dictionary<string, string>
        {
            { "Tokyo", "Visit the Shibuya Crossing and pack comfortable walking shoes!" },
            { "Paris", "The Louvre is closed on Tuesdays; book your tickets in advance." },
            { "Rome", "Avoid the mid-day heat at the Colosseum; drink from the public fountains (Nasoni)." },
            { "New York", "Take the Staten Island Ferry for a free view of the Statue of Liberty." }
        };

        public TravelBot(string name, string version) : base(name, version) { }

        public override void Introduce()
        {
            Console.WriteLine($"[TravelBot] I am {Name}. I can suggest locations and tips for: {string.Join(", ", _destinations.Keys)}");
        }

        public void GetTravelAdvice(string city)
        {
            // Simple logic to find a destination
            var match = _destinations.Keys.FirstOrDefault(k => k.Equals(city, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                Console.WriteLine($"\n--- Travel Tip for {match} ---");
                Console.WriteLine($"Pro-Tip: {_destinations[match]}");
                Console.WriteLine("------------------------------");
            }
            else
            {
                Console.WriteLine($"Sorry, I don't have tips for '{city}' yet. Try Tokyo, Paris, Rome, or New York!");
            }
        }
    }

    public static class BotExtensions
    {
        public static void Reboot(this Bot bot) => Console.WriteLine($"[System] {bot.Name} is rebooting...");
    }

    // --- Main Program ---
    class Program
    {
        static async Task Main(string[] args)
        {
            ChatBot myChatty = new ChatBot("Chatty", "2.1", "English");
            WeatherBot myWeather = new WeatherBot("SkyScan", "3.0", "Woodbridge, NJ");
            TravelBot myTravel = new TravelBot("GlobeTrotter", "1.0");

            bool running = true;
            while (running)
            {
                Console.WriteLine("\n--- Bot Command Center ---");
                Console.WriteLine("1. Talk to ChatBot");
                Console.WriteLine("2. Get Live NJ Weather");
                Console.WriteLine("3. Get Travel Tips");
                Console.WriteLine("4. Exit");
                Console.Write("Choice: ");

                string choice = Console.ReadLine();

                // Using the Switch Expression for navigation
                string status = choice switch
                {
                    "1" => "Chat session started.",
                    "2" => "Weather report requested.",
                    "3" => "Travel guide opened.",
                    "4" => "Exiting...",
                    _ => "Invalid choice."
                };

                Console.WriteLine($"Status: {status}");

                if (choice == "1")
                {
                    myChatty.Introduce();
                    while (true)
                    {
                        Console.Write("You: ");
                        string msg = Console.ReadLine();
                        if (msg.ToLower() == "back") break;
                        myChatty.Chat(msg);
                    }
                }
                else if (choice == "2")
                {
                    await myWeather.GetLiveWeatherAsync();
                }
                else if (choice == "3")
                {
                    myTravel.Introduce();
                    Console.Write("Enter a city: ");
                    string city = Console.ReadLine();
                    myTravel.GetTravelAdvice(city);
                }
                else if (choice == "4")
                {
                    running = false;
                }
            }
        }
    }
}