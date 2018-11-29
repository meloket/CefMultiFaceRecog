using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

using CefSharp;
using CefSharp.WinForms;

namespace CefMulti
{
    public partial class FrmMain : Form
    {
        private VideoCapture _capture;
        private CascadeClassifier _cascadeClassifier;
        private bool _hasRecognizedFace;
        private RecognizerEngine _recognizerEngine;
        private readonly String _databasePath = Application.StartupPath + "/face_store.db";
        private readonly String _trainerDataPath = Application.StartupPath + "/traineddata";
        IDataStoreAccess dataStore;


        public FrmMain(bool fromUserLogin)
        {
            dataStore = new DataStoreAccess(_databasePath);
            InitializeComponent();
            InitBrowser();
            timer1.Enabled = true;
        }

        public ChromiumWebBrowser browser;
        public void InitBrowser()
        {
            CefSharpSettings.SubprocessExitIfParentProcessClosed = true;
            Cef.Initialize(new CefSettings());
            browser = new ChromiumWebBrowser("www.google.com");
            this.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //Train the recognizer here
            _recognizerEngine = new RecognizerEngine(_databasePath, _trainerDataPath);
            lblTrainingStatus.Text = "Training started";
            //btnTrain.Enabled = false;

            Learn();
            //bckGroundTrainer.RunWorkerAsync();

            _cascadeClassifier = new CascadeClassifier(Application.StartupPath + "/haarcascade_frontalface_alt_tree.xml");

            _capture = new VideoCapture();
            //Application.Idle += new EventHandler(ProcessFrame);
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //faceRecognize();
            ProcessFrame();
        }


        private void ProcessFrame(/*Object sender, EventArgs args*/)
        {
            try
            {
                imgCamUser.Image = _capture.QueryFrame();
                if (!_hasRecognizedFace)
                {
                    using (var imageFrame = _capture.QueryFrame().ToImage<Bgr, Byte>())
                    {
                        if (imageFrame != null)
                        {
                            var grayframe = imageFrame.Convert<Gray, byte>();
                            var faces = _cascadeClassifier.DetectMultiScale(grayframe, 1.1, 10, Size.Empty);
                            foreach (var face in faces)
                            {
                                imageFrame.Draw(face, new Bgr(Color.Green), 3);
                                //render the image to the picture box
                                picCapturedUser.Image = imageFrame.Copy(face);
                                var predictedUserId = _recognizerEngine.RecognizeUser(new Image<Gray, byte>(picCapturedUser.Image.Bitmap));
                                //IDataStoreAccess dataStore = new DataStoreAccess(_databasePath);
                                var username = dataStore.GetUsername(predictedUserId);
                                if (username != String.Empty)
                                {
                                    CvInvoke.PutText(imageFrame, username, new Point(face.Location.X + 10, face.Location.Y - 10), Emgu.CV.CvEnum.FontFace.HersheyComplex, 1.0, new Bgr(0, 255, 0).MCvScalar);
                                }
                            }
                        }
                        imgCamUser.Image = imageFrame;
                        //if (picCapturedUser.Image != null) _hasRecognizedFace = true;
                    }
                }
            }
            catch
            {
                //MessageBox.Show("Camera Error!");
                Application.Exit();
            }
            
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            picCapturedUser.Image = null;
            _hasRecognizedFace = false;
        }

        public static byte[] ConvertImageToByte(Image<Gray, byte> myImage)
        {
            MemoryStream ms = new MemoryStream();
            new Bitmap(myImage.ToBitmap()).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            byte[] header = new byte[] { 255, 216 };
            header = ms.ToArray();
            return (header);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var faceToSave = new Image<Gray, byte>(picCapturedUser.Image.Bitmap);

            if (faceToSave == null)
            {
                MessageBox.Show("No face recognized!");
                return;
            }

            Byte[] file;
            //IDataStoreAccess dataStore = new DataStoreAccess(_databasePath);
            var saveName = textBox1.Text;
            file = ConvertImageToByte(faceToSave);
            var result = dataStore.SaveFace(saveName, file);
            MessageBox.Show(result, "Save Result", MessageBoxButtons.OK);
            Learn();

            /*var frmSaveDialog = new FrmSaveDialog();
            if (frmSaveDialog.ShowDialog() == DialogResult.OK)
            {
                if (frmSaveDialog._identificationNumber.Trim() != String.Empty)
                {
                    var username = frmSaveDialog._identificationNumber.Trim().ToLower();
                    var filePath = Application.StartupPath + String.Format("/{0}.bmp", username);
                    faceToSave.ToBitmap().Save(filePath);
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new BinaryReader(stream))
                        {
                            file = reader.ReadBytes((int)stream.Length);
                        }
                    }
                    var result = dataStore.SaveFace(username, file);
                    MessageBox.Show(result, "Save Result", MessageBoxButtons.OK);
                }

            }*/
        }

        

        private bool Learn()
        {
            var hasTrainedRecognizer = _recognizerEngine.TrainRecognizer();
            return hasTrainedRecognizer;
        }

        /*private bool TrainRecognizer(BackgroundWorker worker, DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                var hasTrainedRecognizer = _recognizerEngine.TrainRecognizer();
                return hasTrainedRecognizer;
            }
            return false;
        }

        private void bckGroundTrainer_DoWork(object sender, DoWorkEventArgs e)
        {

            var worker = sender as BackgroundWorker;
            e.Result = TrainRecognizer(worker, e);
        }

        private void bckGroundTrainer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                lblTrainingStatus.Text = "Training has been cancelled!";
            }
            else
            {
                var result = (bool)e.Result;
                lblTrainingStatus.Text = result ? "Training has been completed successfully!" : "Training could not be completed, An Error Occurred";
            }
            //btnTrain.Enabled = true;
        }*/

        /*private void btnTrain_Click(object sender, EventArgs e)
        {
            //btnTrain.Enabled = false;
            bckGroundTrainer.RunWorkerAsync();
        }*/

        

        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            _capture.Stop();
            _capture.Dispose();
            //Application.Idle -= ProcessFrame;
        }
    }
}
