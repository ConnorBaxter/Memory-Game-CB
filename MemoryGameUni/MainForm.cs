using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MemoryGameUni
{
    public partial class MainForm : Form
    {
        /*
         * DONE:
         *  - Add fruit class 
         *  - Keep list of fruits
         *  - Keep list of picture boxes
         *  - Add images to picture boxes
         *  - Get random fruit to guess
         *  - Get random fruit to guess from
         *  - Add win and loss
         *  - option to play again
         *  - countdown timer per round
         *  - Made sure while random right answer alaways appears
         *  - 
         *
         * TODO:
         *  - Count how many seconds user takes to guess a fruit
         *  - Keep track of how many wins / losses a user has this session
         *  - Add a scoring system?
         *  - Add more fruit or images
         *  - Improve BlankImage by keeping it as a path and not image (this will save RAM usage)
         *  - Add difficulty settings (time between rounds)
         *  - Improve the layout of the form to look nicer and add colour
         *  - Fix LoadFruit so the Fruits dont memory hog
         */

        //List (an array basically) to keep all fruit (see fruit class below)
        List<Fruit> FruitList = new List<Fruit>();

        //Keep a list of the picture boxes so we can put images in them later
        List<PictureBox> PictureBoxes = new List<PictureBox>();

        //we will need this later do displays of random fruits
        private List<Fruit> fruitsToDisplay;

        //Keep a copy of the fruit were going to try and guess
        private Fruit fruitToGuess;

        //Keep a record of if the images are hidden at this time
        private bool isHidden = false;

        //Keep an image called blank that we can then put into picture boxes
        //to hide what was there before
        Image BlankImage = Image.FromFile(GetPath("_Blank"));

        //Make an instance of the random class to get random numbers
        Random rand = new Random();

        //Make a timer that will wait until
        //we will initalise this timer later
        Timer HideTimer = new Timer();

        //make another countdown timer to 
        //display countdown to hide times
        Timer TimeLeftTimer = new Timer();

        public MainForm()
        {
            InitializeComponent();
        }

        //When the form loads this code will run
        private void MainForm_Load(object sender, EventArgs e)
        {
            //Load in the fruits
            LoadFruit();

            //loop through the controls in the panel and add all picture boxes 
            //to our list of picture boxes
            foreach (Control cntrl in pnlImageList.Controls)
            {
                //if the control is a picture box then add it
                if (cntrl is PictureBox)
                {
                    //Here we cast (tell the compiler what type it is) the control to a picture box
                    //and then add it to the list
                    PictureBoxes.Add((PictureBox) cntrl);
                }
            }

            //initalise the timer but dont start it yet
            //1000ms = 1 second
            //times by 7 so its then 7 seconds
            //change 7 here to change the time to memorise the items
            //this means you need to change the countdown timer too
            //maybe a function to change difficulty?
            HideTimer.Interval = (1000) * 7;
            HideTimer.Tick += HideTimer_Tick;

            //initalise the timer to tick every second
            TimeLeftTimer.Interval = 1000;
            TimeLeftTimer.Tick += TimeLeftTimer_Tick;
        }

        //Return the file path of an image
        private static string GetPath(string filename)
        {
            //This puts the filename between the current working directory and then appends .png
            /*
             * This basically turns "Pear" into a valid path to wherever this code is running
             * i.e on my pc "Pear" becomes "C:/Users/Connor/Source/repos/MemoryGameUni/Images/Pear.png"
             * without me having to copy and paste the path for each image
             * if it was on my desktop it would be "C:/Users/Connor/Desktop/MemoryGameUni/Images/Pear.png"
             * This means we dont need to know where exactly the file is to use it.
             * This should work on every computer (should)
             */
            return Environment.CurrentDirectory + "/Images/" + filename + ".png";
        }

        //Load all the fruits into the list
        private void LoadFruit()
        {
            //Make a new "fruit" for each image, give it a path and a name
            //probably an easier way to do this as these are memory hogging
            //and only used until they are added to the list
            //Each one of these is an instance of the class
            Fruit Apple = new Fruit(GetPath("Apple"), "Apple");
            Fruit Banana = new Fruit(GetPath("Banana"), "Banana");
            Fruit Berries = new Fruit(GetPath("Berries"), "Berries");
            Fruit Cherries = new Fruit(GetPath("Cherries"), "Cherries");
            Fruit Chilli = new Fruit(GetPath("Chilli"), "Chilli");
            Fruit Eggplant = new Fruit(GetPath("Eggplant"), "Eggplant");
            Fruit Orange = new Fruit(GetPath("Orange"), "Orange");
            Fruit Pear = new Fruit(GetPath("Pear"), "Pear");
            Fruit Pineapple = new Fruit(GetPath("Pineapple"), "Pineapple");
            Fruit Pumpkin = new Fruit(GetPath("Pumpkin"), "Pumpkin");
            Fruit Strawberries = new Fruit(GetPath("Strawberries"), "Strawberries");
            Fruit Tomato = new Fruit(GetPath("Tomato"), "Tomato");
            Fruit Watermelon = new Fruit(GetPath("Watermelon"), "Watermelon");

            //Clear the list just incase it already has items in it
            //if not everytime the user plays again the list would get bigger
            FruitList.Clear();

            //Now add all the "fruits" into the global list 
            FruitList.AddRange( new []
            {
                Apple,
                Banana,
                Berries,
                Cherries,
                Chilli,
                Eggplant,
                Orange,
                Pear,
                Pineapple,
                Pumpkin,
                Strawberries,
                Tomato,
                Watermelon
            });
        }

        //this code is ran every time a image of fruit is clicked on
        private void FruitClicked(object sender, EventArgs e)
        {
            //if the fruit isnt hidden yet then just ignore the click
            //as its useless at the minute
            if(!isHidden) return;

            //if the name of the fruit weve just clicked is the same name as the fruit
            //we need to guess then the user guessed right
            //here we get the fruit names of the fruit to guess and the fruit
            //the user guessed
            string fruitName = fruitToGuess.FruitName;
            string guessName = GetFruitFromPictureBox((PictureBox) sender).FruitName;
            
            //this means the user won
            if (fruitName == guessName)
            {
                EndGame(true);
            }
            else
            {
                //if not the user lost
                EndGame(false);
            }
        }

        //End the game by asking if you want to play again or quit
        private void EndGame(bool won)
        {
            //show all images so the user can see where
            //things are
            //same as code below where we first display this
            int counter = 0;
            foreach (PictureBox pb in PictureBoxes)
            {
                pb.Image = fruitsToDisplay[counter].GetAsImage();
                counter++;
            }

            //Set the text for if the user won or not
            //and ask if theyd like to play again
            string msgText;
            if (won)
            {
                msgText = "You won!";
            }
            else
            {
                msgText = "You lost loser!";
            }
            msgText += " \nWould you like to play again?";

            //Here we show a dialog with the option to press yes or no
            //we then use the result to whether or not the user wants to play again
            DialogResult result = MessageBox.Show(msgText, "Memory Game", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                //start the game again
                //the null options are because this expects to be called
                //from a button but we are calling it ourselfs
                //but this needs a sender and event arguemnets
                // so we pass null so its still valid
                StartGame(null, null);
            }
            else
            {
                //Quit and close the program
                Application.Exit();
            }
        }

        //Get the fruit from the picturebox
        private Fruit GetFruitFromPictureBox(PictureBox pb)
        {
            //get the path of the image file
            string imgLocation = pb.ImageLocation;

            //use the method from fruit to convert a filepath to a fruit instance
            Fruit f = new Fruit().GetFruitFromImageLocation(imgLocation);
            return f;
        }

        //runs when start new game button is pressed or we start again from the end screen
        private void StartGame(object sender, EventArgs e)
        {
            //Stop these incase new game was started mid-round
            HideTimer.Stop();
            TimeLeftTimer.Stop();

            //Reset the time label to 7 seconds
            lblTimer.Text = "Time Left: 7";
            //Reset the label for the guess
            lblFruitToGuess.Text = "Click the:";

            //hide the last fruit if the game has already been played
            pbFruitGuess.Image = BlankImage;

            //Probably dont need to reload the fruits but better safe than sorry
            LoadFruit();

            //Get a random fruit from the list of fruits
            //this gets a random number from 0 to the amount of fruit in the list
            //if the list contains 10 items this will get a random number between 0 and 10
            //and then get that index of the FruitList and copys it into fruitToGuess
            fruitToGuess = FruitList[rand.Next(0, FruitList.Count)];

            //put the rest of the images int the grid
            DisplayAllFruit();

            //Start the timer to countdown and hide the photos
            HideTimer.Start();
            TimeLeftTimer.Start();
        }

        //runs every 7 seconds when timer is ticking
        private void HideTimer_Tick(object sender, EventArgs e)
        {
            //Hide the fruit after this timer has finished and 
            // then stop the timer so it doesnt run twice
            HideFruit();
            HideTimer.Stop();

            //Put the image of the fruit we just picked into the picture box 
            //so the user knows what to look for
            DisplayFruitToGuess();
        }

        //runs every second while timer is ticking
        private void TimeLeftTimer_Tick(object sender, EventArgs e)
        {
            //Get the seconds left from the labels text
            string timeAsStr = lblTimer.Text.Split(' ')[2];
            //Convert it to an integer
            int secondsLeft = int.Parse(timeAsStr);
            //take 1 away from the seconds left
            secondsLeft--;

            //If the seconds are equal or below zero stop the timer
            //and quit
            if (secondsLeft <= 0)
            {
                lblTimer.Text = "Time Left: 0";
                TimeLeftTimer.Stop();
                return;
            }

            //Set the seconds to display on the label
            lblTimer.Text = $"Time Left: {secondsLeft}";
        }

        //display the fruit we need to guess
        private void DisplayFruitToGuess()
        {
            //Here we use the fruit class GetAsImage function to get a bitmap from the hard drive
            //and put it in the guessing picture box 
            pbFruitGuess.Image = fruitToGuess.GetAsImage();

            //set the label to contain the fruit name
            lblFruitToGuess.Text = $"Click the: {fruitToGuess.FruitName}";
        }

        //Display all the fruit in the grid
        private void DisplayAllFruit()
        {
            //create a temporary list to store some fruits in
            fruitsToDisplay = new List<Fruit>();

            //make sure that the fruit to guess is in the list
            fruitsToDisplay.Add(fruitToGuess);

            //add 8 more random images to the list
            fruitsToDisplay.AddRange(new []
            {
                FruitList[rand.Next(0, FruitList.Count)],
                FruitList[rand.Next(0, FruitList.Count)],
                FruitList[rand.Next(0, FruitList.Count)],
                FruitList[rand.Next(0, FruitList.Count)],
                FruitList[rand.Next(0, FruitList.Count)],
                FruitList[rand.Next(0, FruitList.Count)],
                FruitList[rand.Next(0, FruitList.Count)],
                FruitList[rand.Next(0, FruitList.Count)]
            });

            //shuffle the list, this is a hacky way to randomise the list
            //im not 100% sure how it works but it basically gives each thing a random id
            //and then sorts the ids highest to lowest so its not fully random but random enough
            fruitsToDisplay = fruitsToDisplay.OrderBy(a => Guid.NewGuid()).ToList();

            //create a counter and then loop through the picture boxes and add a now random image
            //then increment the counter by one each time
            int counter = 0;
            foreach (PictureBox pb in PictureBoxes)
            {
                //Set the image to the image of the fruit
                pb.Image = fruitsToDisplay[counter].GetAsImage();
                //Set the path to the image as the path to the fruit
                //this doesnt affect the image
                //so could have the image as an apple but a 
                //path to banana and it would show an apple
                //weird behavior but its useful for us
                pb.ImageLocation = fruitsToDisplay[counter].Path;
                //increment the counter
                //should probably use a for statement instead of foreach
                //but im lasy and this works
                counter++;
            }

            //make is hidden false as they are now shown
            isHidden = false;
        }

        //Hide all the fruit in the grid
        private void HideFruit()
        {
            //Loop through each picture box
            foreach (PictureBox pb in PictureBoxes)
            {
                //turn the image to the blank image
                //leave the ImageLocation as the same so
                //we can use that to work out what was there before
                //the image location doesnt affect what is shown
                //I dont know why it just doesnt
                pb.Image = BlankImage;
            }

            //make is hidden true as they are now hidden
            isHidden = true;
        }
    }

    //Should put classes in another file but put here so its easier to read
    class Fruit
    {
        /*
         * Here we give each fruit a path and a name
         * The path is the image files location on your hard drive
         * The name is used later for if statements and such
         */

        //Class variables
        public string Path;
        public string FruitName;


        //The class constructor to make a "fruit"
        public Fruit(string path, string fruitName)
        {
            //Set the class variables to equal the local constructor variables
            Path = path;
            FruitName = fruitName;
        }

        //Another constrcutor so we can make blank fruit classes
        //this is so we can use methods from this class without needing to 
        //make a fruit first
        public Fruit() { }

        public Fruit GetFruitFromImageLocation(string location)
        {
            //remove the ".png" from the filename
            string name = location.Split('.')[0];

            //Very very hacky way to get a name of the fruit from the filename
            //split the string by the "/" path seperator and get the last one
            //so "C://Users/Connor/Source/repos/MemoryGameUni/Images/Pear.png"
            //becomes an array containing [C, Users, Connor, ... , Pear] and the
            //last item is "Pear" so then we set that as the name
            name = name.Split(new [] { "/" }, StringSplitOptions.RemoveEmptyEntries).Last();

            Fruit f = new Fruit(location, name);
            return f;
        }

        //Get the image from the path variable
        public Image GetAsImage()
        {
            Image img = new Bitmap(Path);
            return img;
        }

        //Return this instance as a fileinfo type, useful to get the location of a file
        //we might not need this but its useful to have
        public FileInfo GetAsFileInfo()
        {
            FileInfo temp = new FileInfo(Path);
            return temp;
        }
    }
}
