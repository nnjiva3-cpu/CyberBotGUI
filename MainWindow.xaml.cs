using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CyberBotGUI
{
    public partial class MainWindow : Window
    {
        // Delegates
        public delegate string ResponseHandler(string input);
        public delegate void MessageDisplay(string message);
        private ResponseHandler _responseHandler;
        private MessageDisplay _displayHandler;

        private string _userName = "";
        private string _lastTopic = "";
        private string _favoriteTopic = "";
        private Dictionary<string, List<string>> _responses;
        private bool _nameAsked = false;
        private Random _rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
            _responseHandler = new ResponseHandler(GetResponse);
            _displayHandler = new MessageDisplay(AddBotMessage);
            InitializeResponses();
            PlayVoiceGreeting();
            AskForName();
        }

        private void PlayVoiceGreeting()
        {
            try
            {
                string audioPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");
                if (File.Exists(audioPath))
                {
                    SoundPlayer player = new SoundPlayer(audioPath);
                    player.Play();
                }
            }
            catch { }
        }

        private void InitializeResponses()
        {
            _responses = new Dictionary<string, List<string>>
            {
                ["password"] = new List<string>
                {
                    "Use at least 12 characters mixing uppercase, lowercase, numbers and symbols!",
                    "Never reuse passwords across different sites — use a password manager like Bitwarden!",
                    "Avoid using personal info like birthdays or names in your passwords.",
                    "Change your passwords every 3-6 months for important accounts.",
                    "A passphrase like 'PurpleCat$RunsFast99' is both strong and memorable!"
                },
                ["phishing"] = new List<string>
                {
                    "Always check the sender's email address carefully before clicking any links!",
                    "Legitimate companies will NEVER ask for your password via email.",
                    "Hover over links to preview the URL before clicking — look for misspellings!",
                    "If an email creates urgency like 'Act now or lose access', it's likely a scam.",
                    "When in doubt, go directly to the website instead of clicking email links."
                },
                ["browsing"] = new List<string>
                {
                    "Always look for HTTPS and the padlock icon before entering personal info!",
                    "Use a VPN when connected to public Wi-Fi networks.",
                    "Keep your browser and extensions updated to patch security vulnerabilities.",
                    "Use an ad blocker to reduce exposure to malicious advertisements.",
                    "Clear your cookies and cache regularly to protect your privacy."
                },
                ["malware"] = new List<string>
                {
                    "Install reputable antivirus software and keep it updated at all times!",
                    "Never download software from untrusted or unofficial sources.",
                    "Be cautious with email attachments — even from people you know!",
                    "Back up your data regularly to protect against ransomware attacks.",
                    "Ransomware can encrypt all your files — always have offline backups!"
                },
                ["privacy"] = new List<string>
                {
                    "Review your privacy settings on all social media accounts regularly!",
                    "Limit the personal information you share publicly online.",
                    "Use a VPN to encrypt your internet traffic and protect your identity.",
                    "Be careful what you post — once online, it can be very hard to remove.",
                    "Use two-factor authentication on all your important accounts!"
                },
                ["scam"] = new List<string>
                {
                    "Be suspicious of unsolicited calls or emails asking for personal info!",
                    "Scammers create urgency — slow down and verify before acting.",
                    "If something sounds too good to be true, it probably is!",
                    "Never send money or gift cards to someone you haven't met in person.",
                    "Verify the identity of anyone requesting sensitive data by calling them directly."
                },
                ["2fa"] = new List<string>
                {
                    "Two-factor authentication adds an extra layer of security beyond your password!",
                    "Use an authenticator app like Google Authenticator instead of SMS for 2FA.",
                    "Enable 2FA on all important accounts — email, banking, and social media.",
                    "Even if someone steals your password, 2FA stops them from logging in!"
                }
            };
        }

        private void AskForName()
        {
            _displayHandler("Welcome to the Cybersecurity Awareness Bot! 🔐");
            _displayHandler("I'm here to help you stay safe in the digital world.");
            _displayHandler("Before we begin, what is your name?");
            _nameAsked = true;
        }

        private void TopicButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button != null)
            {
                string topic = button.Content.ToString()
                    .Replace("🔑 ", "").Replace("🎣 ", "")
                    .Replace("🌐 ", "").Replace("🦠 ", "")
                    .Replace("🔒 ", "").Replace("⚠️ ", "")
                    .ToLower().Trim();

                if (topic == "passwords") topic = "password";
                if (topic == "safe browsing") topic = "browsing";
                if (topic == "scams") topic = "scam";

                InputBox.Text = topic;
                ProcessInput();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessInput();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ProcessInput();
        }

        private void ProcessInput()
        {
            string input = InputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                _displayHandler("⚠️ Please type something! I didn't receive any input.");
                return;
            }

            AddUserMessage(input);
            InputBox.Clear();

            if (_nameAsked && _userName == "")
            {
                _userName = input;
                _nameAsked = false;
                _displayHandler($"Nice to meet you, {_userName}! 😊");
                _displayHandler("I'm CyberBot, your personal cybersecurity guide.");
                _displayHandler("Click a topic button above or ask me anything! Type 'help' to see all topics.");
                return;
            }

            string response = _responseHandler(input.ToLower());
            _displayHandler(response);
        }

        private string GetResponse(string input)
        {
            // Sentiment detection
            if (input.Contains("worried") || input.Contains("scared") || input.Contains("anxious"))
                return $"I completely understand, {_userName}. Feeling worried about cybersecurity is very normal! The good news is that a few simple habits can keep you very safe. Here's a tip to start:\n\n" + GetRandomResponse("scam");

            if (input.Contains("frustrated") || input.Contains("confused") || input.Contains("don't understand") || input.Contains("lost"))
                return $"No worries at all, {_userName}! Let's slow down and take it step by step. Here's something simple:\n\n" + GetRandomResponse("password");

            if (input.Contains("curious") || input.Contains("interested") || input.Contains("want to learn"))
            {
                if (_favoriteTopic == "") _favoriteTopic = "cybersecurity";
                return $"I love the curiosity, {_userName}! That's the best first step to staying safe online. Here's an interesting tip:\n\n" + GetRandomResponse("privacy");
            }

            if (input.Contains("thank") || input.Contains("thanks"))
                return $"You're welcome, {_userName}! Stay safe out there! 🔐 Is there anything else you'd like to know?";

            // Memory and recall
            if (input.Contains("tell me more") || input.Contains("explain more") || input.Contains("more info") || input.Contains("give me another") || input.Contains("another tip"))
            {
                if (_lastTopic != "")
                    return $"Here's another tip on {_lastTopic}:\n\n" + GetRandomResponse(_lastTopic);
                return $"What topic would you like more information about, {_userName}?";
            }

            // Favourite topic recall
            if (input.Contains("what do i like") || input.Contains("my favourite") || input.Contains("remember me"))
            {
                if (_favoriteTopic != "")
                    return $"I remember you're interested in {_favoriteTopic}, {_userName}! Here's another tip:\n\n" + GetRandomResponse(_favoriteTopic);
                return $"I don't have a favourite topic saved for you yet, {_userName}. Ask me about any topic first!";
            }

            // Help
            if (input.Contains("help") || input.Contains("topics") || input.Contains("what can"))
                return $"Here are all the topics I can help you with, {_userName}:\n\n" +
                       "🔑 Passwords\n🎣 Phishing\n🌐 Safe Browsing\n" +
                       "🦠 Malware\n🔒 Privacy\n⚠️ Scams\n🔐 2FA\n\n" +
                       "Just type any topic or click the buttons above!";

            // Greetings
            if (input.Contains("hello") || input.Contains("hi") || input.Contains("hey"))
                return $"Hey {_userName}! 👋 Great to see you! How can I help you stay cyber-safe today?";

            if (input.Contains("how are you"))
                return $"I'm running at full security capacity, {_userName}! 😄 Ready to help you stay safe online!";

            if (input.Contains("purpose") || input.Contains("what do you do") || input.Contains("who are you"))
                return $"I'm CyberBot, {_userName}! My purpose is to educate you on cybersecurity topics and help you stay safe in the digital world!";

            // Topic responses
            foreach (var topic in _responses.Keys)
            {
                if (input.Contains(topic))
                {
                    _lastTopic = topic;
                    if (_favoriteTopic == "") _favoriteTopic = topic;
                    return $"Here's a tip on {topic}, {_userName}:\n\n" + GetRandomResponse(topic) +
                           "\n\nType 'tell me more' for another tip on this topic!";
                }
            }

            // Exit
            if (input.Contains("bye") || input.Contains("exit") || input.Contains("goodbye"))
                return $"Goodbye {_userName}! It was great chatting with you. Stay safe online! 🔐";

            // Default fallback
            return $"I didn't quite catch that, {_userName}. 🤔 Try asking about passwords, phishing, malware, privacy or scams. Type 'help' to see all topics!";
        }

        private string GetRandomResponse(string topic)
        {
            var list = _responses[topic];
            return list[_rand.Next(list.Count)];
        }

        private void AddBotMessage(string message)
        {
            var para = new Paragraph();
            var run = new Run($"🤖 CyberBot: {message}\n");
            run.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136));
            para.Inlines.Add(run);
            ChatBox.Document.Blocks.Add(para);
            ChatScrollViewer.ScrollToBottom();
        }

        private void AddUserMessage(string message)
        {
            var para = new Paragraph();
            var run = new Run($"👤 {_userName}: {message}\n");
            run.Foreground = new SolidColorBrush(Color.FromRgb(135, 206, 250));
            para.Inlines.Add(run);
            ChatBox.Document.Blocks.Add(para);
            ChatScrollViewer.ScrollToBottom();
        }
    }
}
