using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace WinFormsAppFace
{
    public partial class Form1 : Form
    {
        public List<BoundingBox> values = new List<BoundingBox>();
        public List<TextDetection> textDetections = new List<TextDetection>();
        string photo = "face.jpg";
        string bucket = "forimagebucket";
        BasicAWSCredentials credentials = new BasicAWSCredentials("********", "*************");
        
        public Form1()
        {
            InitializeComponent();
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient(credentials, Amazon.RegionEndpoint.USEast1);
            var detectFacesRequest = new DetectFacesRequest()
            {
                Image = new Amazon.Rekognition.Model.Image()
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object()
                    {
                        Name = photo,
                        Bucket = bucket,
                    },
                },

                // Attributes can be "ALL" or "DEFAULT".
                // "DEFAULT": BoundingBox, Confidence, Landmarks, Pose, and Quality.
                // "ALL": See https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/Rekognition/TFaceDetail.html
                Attributes = new List<string>() { "DEFAULT" },
            };
            // Create a client
            AmazonS3Client client = new AmazonS3Client();

            // Create a GetObject request
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = "forimagebucket",
                Key = "face.jpg"
            };

            // Issue request and remember to dispose of the response
            using (GetObjectResponse response = await client.GetObjectAsync(request))
            {
                // Save object to local file
                await response.WriteResponseStreamToFileAsync("Item1.txt", false, new CancellationTokenSource().Token);
            }
            pictureBox1.Image = System.Drawing.Image.FromFile("Item1.txt");
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            try
            {
                DetectFacesResponse detectFacesResponse = await rekognitionClient.DetectFacesAsync(detectFacesRequest);
                bool hasAll = detectFacesRequest.Attributes.Contains("ALL");
                foreach (FaceDetail face in detectFacesResponse.FaceDetails)
                {
                    textBox1.Text = $"BoundingBox: top={face.BoundingBox.Left} left={face.BoundingBox.Top}" +
                        $" width={face.BoundingBox.Width} height={face.BoundingBox.Height}\t" +
                        $"Confidence: {face.Confidence}\t" +
                        $"Landmarks: {face.Landmarks.Count}\t" +
                        $"Pose: pitch={face.Pose.Pitch}\t roll={face.Pose.Roll}\t yaw={face.Pose.Yaw}\t" +
                        $"Brightness: {face.Quality.Brightness}\tSharpness: {face.Quality.Sharpness}";

                    if (hasAll)
                    {
                        Console.WriteLine($"Estimated age is between {face.AgeRange.Low} and {face.AgeRange.High} years old.");
                    }
                    Bitmap bmp = new Bitmap(pictureBox1.Image);
                    using (Graphics g = Graphics.FromImage(bmp))
                    using (Pen pen = new Pen(Color.Red, 2))
                    {
                        int left = (int)(face.BoundingBox.Left * bmp.Width);
                        int top = (int)(face.BoundingBox.Top * bmp.Height);
                        int width = (int)(face.BoundingBox.Width * bmp.Width);
                        int height = (int)(face.BoundingBox.Height * bmp.Height);

                        Rectangle rect = new Rectangle(left, top, width, height);
                        g.DrawRectangle(pen, rect);
                    }
                    pictureBox1.Image = bmp;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            // Create a client
            AmazonS3Client client = new AmazonS3Client();

            // Create a GetObject request
            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = "forimagebucket",
                Key = "abrakadabra.jpg"
            };

            // Issue request and remember to dispose of the response
            using (GetObjectResponse response = await client.GetObjectAsync(request))
            {
                // Save object to local file
                await response.WriteResponseStreamToFileAsync("Item1.txt", false, new CancellationTokenSource().Token);
            }
            pictureBox1.Image = System.Drawing.Image.FromFile("Item1.txt");
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
            DetecteText(textBox1, pictureBox1, credentials);
        }

        public static async Task DetecteText(TextBox textBox1, PictureBox pictureBox1, BasicAWSCredentials credentials)
        {
            String photo = "abrakadabra.jpg";
            String bucket = "forimagebucket";
            AmazonRekognitionClient rekognitionClient = new AmazonRekognitionClient(credentials, Amazon.RegionEndpoint.USEast1);

            DetectTextRequest detectTextRequest = new DetectTextRequest()
            {
                Image = new Amazon.Rekognition.Model.Image()
                {
                    S3Object = new Amazon.Rekognition.Model.S3Object()
                    {
                        Name = photo,
                        Bucket = bucket
                    }
                }
            };

            try
            {
                DetectTextResponse detectTextResponse = await rekognitionClient.DetectTextAsync(detectTextRequest);
                textBox1.Text = $"Detected lines and words for {photo}";
                Bitmap bmp = new Bitmap(pictureBox1.Image);

                using (Graphics g = Graphics.FromImage(bmp))
                using (Pen pen = new Pen(Color.Red, 2))
                using (Pen pen2 = new Pen(Color.Green, 2))
                {
                    foreach (TextDetection text in detectTextResponse.TextDetections)
                    {
                        //textBox1.Text = $"Detected: {text.DetectedText}";
                        Console.WriteLine("Detected: " + text.DetectedText);
                        Console.WriteLine("Confidence: " + text.Confidence);
                        Console.WriteLine("Id : " + text.Id);
                        Console.WriteLine("Parent Id: " + text.ParentId);
                        Console.WriteLine("Type: " + text.Type);
                        BoundingBox box = text.Geometry.BoundingBox;

                        int left = (int)(box.Left * bmp.Width);
                        int top = (int)(box.Top * bmp.Height);
                        int width = (int)(box.Width * bmp.Width);
                        int height = (int)(box.Height * bmp.Height);

                        Rectangle rect = new Rectangle(left, top, width, height);

                        if (text.Type == "LINE")
                            g.DrawRectangle(pen, rect);
                        else
                            g.DrawRectangle(pen2, rect);
                    }
                }
                pictureBox1.Image = bmp;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

