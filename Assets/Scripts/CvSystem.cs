using System;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using UnityEngine;
using KartGame.KartSystems;


[Serializable]
public struct Triple
{
    public double h, s, v;
}

public class CvSystem : MonoBehaviour
{
    //public CameraInput cameraInput;
    public int rotationBufferSize, positionBufferSize;
    public Triple leftHsvMin, leftHsvMax;

    private VideoCapture webcam;
    private InputBuffer<float> rotations;
    private InputBuffer<Vector3> positions;
    private Mat imgBgr, imgIn;
    private Hsv leftMarkerMin, leftMarkerMax;

    public bool Brake { get; internal set; }
    public bool Accelerate { get; internal set; }
    public float SteeringAngle { get; internal set; }
    public bool Hop { get; set; }

    void Start()
    {
        webcam = new VideoCapture();
        webcam.FlipHorizontal = true;
        CvInvoke.CheckLibraryLoaded();

        if (rotationBufferSize % 2 == 0)
            rotations = new InputBuffer<float>(rotationBufferSize);
        else
            rotations = new InputBuffer<float>(rotationBufferSize + 1);

        positions = new InputBuffer<Vector3>(positionBufferSize);

        leftMarkerMin = MakeHsv(leftHsvMin);
        leftMarkerMax = MakeHsv(leftHsvMax);

        webcam.ImageGrabbed += Webcam_ImageGrabbed;
    }

    private void Update()
    {
        if (webcam.IsOpened) webcam.Grab();
    }

    private void Webcam_ImageGrabbed(object sender, EventArgs e)
    {
        if (!webcam.IsOpened) return;

        imgBgr = new Mat();
        webcam.Retrieve(imgBgr);
        imgIn = imgBgr.Clone();

        CvInvoke.CvtColor(imgIn, imgIn, ColorConversion.Bgr2Hsv);
        CvInvoke.GaussianBlur(imgIn, imgIn, new Size(25, 25), 0);

        var rectangle = GetColorRectangle(leftMarkerMin, leftMarkerMax);
        Accelerate = false;
        Brake = false;
        if (rectangle.HasValue)
        {
            // Calculate new angle average
            float angle = rectangle.Value.Angle;
            if (rectangle.Value.Size.Height > rectangle.Value.Size.Width)
            {
                angle += 90;
            }
            rotations.PushBack(angle);
            float angleAverage = 0.0f;
            foreach (var f in rotations.data)
                angleAverage += f;
            angleAverage /= rotations.curLength;
            SteeringAngle = angleAverage/90;
            Vector3 currentCenter = new Vector2(rectangle.Value.Center.X, rectangle.Value.Center.Y);

            positions.PushBack(currentCenter);
            // Calculate new delta-positions average
            var pedal = (rectangle.Value.Size.Height + rectangle.Value.Size.Width);
            Debug.Log(currentCenter + " " + SteeringAngle+ " " 
                      + pedal);
            Accelerate = (pedal < 160)? true : false ;
            Brake = (pedal > 220 )? true:false ;
           
        }



    }

    void DrawPointsFRectangle(PointF[] boundRecPoints, Mat output)
    {
        // Draw Bounding Rectangle from the first 4 points of "boundRecPoints" onto "output"
        CvInvoke.Line(output, new Point((int)boundRecPoints[0].X, (int)boundRecPoints[0].Y), new Point((int)boundRecPoints[1].X, (int)boundRecPoints[1].Y), new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
        CvInvoke.Line(output, new Point((int)boundRecPoints[1].X, (int)boundRecPoints[1].Y), new Point((int)boundRecPoints[2].X, (int)boundRecPoints[2].Y), new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
        CvInvoke.Line(output, new Point((int)boundRecPoints[2].X, (int)boundRecPoints[2].Y), new Point((int)boundRecPoints[3].X, (int)boundRecPoints[3].Y), new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
        CvInvoke.Line(output, new Point((int)boundRecPoints[3].X, (int)boundRecPoints[3].Y), new Point((int)boundRecPoints[0].X, (int)boundRecPoints[0].Y), new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
    }

    private RotatedRect? GetColorRectangle(Hsv min, Hsv max)
    {
        var orangeMaybe = imgIn.ToImage<Hsv, byte>().InRange(min, max);

        int operationSize = 1;
        var structuringElement = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(2 * operationSize + 1, 2 * operationSize + 1), new Point(operationSize, operationSize));

        CvInvoke.Erode(orangeMaybe, orangeMaybe, structuringElement, new Point(-1, -1), 5, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Dilate(orangeMaybe, orangeMaybe, structuringElement, new Point(-1, -1), 5, BorderType.Constant, new MCvScalar(0));

        var contours = new VectorOfVectorOfPoint();
        var contour = new VectorOfPoint();
        var hierarchy = new Mat();
        CvInvoke.FindContours(orangeMaybe, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);
        var biggestContourArea = 0.0;
        for (int i = 0; i < contours.Size; i++)
        {
            double a = CvInvoke.ContourArea(contours[i], false);
            if (a > biggestContourArea)
            {
                biggestContourArea = a;
                contour = contours[i];
            }
        }

        if (contour.Size > 0)
        {
            var boundRec = CvInvoke.MinAreaRect(contour);
            DrawPointsFRectangle(boundRec.GetVertices(), orangeMaybe.Mat);
            CvInvoke.Imshow("cam " + min.ToString(), orangeMaybe);
            CvInvoke.WaitKey(24);
            return boundRec;
        }

        return null;
    }

    private Hsv MakeHsv(Triple t)
    {
        return new Hsv(t.h / 2.0, t.s, t.v);
    }

    private void OnDestroy()
    {
        webcam.Dispose();
        CvInvoke.DestroyAllWindows();
    }
}
