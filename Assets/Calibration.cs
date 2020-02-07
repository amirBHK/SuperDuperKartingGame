using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using UnityEngine;
using KartGame.KartSystems;


public class Calibration : MonoBehaviour
{
    public float AccelerateTr;
    public float BrakeTr;

    public float Hoptr;

    //public CameraInput cameraInput;
    public int rotationBufferSize, positionBufferSize;
    public Triple leftHsvMin, leftHsvMax;
    public Triple rightHsvMin, rightHsvMax;

    private VideoCapture webcam;
    private InputBuffer<float> rotations;
    private InputBuffer<Vector3> positions;
    private Mat imgBgr, imgIn;
    private Hsv leftMarkerMin, leftMarkerMax;
    private Hsv rightMarkerMin, rightMarkerMax;
    private float range;

    public bool Brake { get; private set; }
    public float Accelerate { get; private set; }
    public float SteeringAngle { get; private set; }
    public bool HopHeld { get; private set; }
    public bool HopPressed { get; private set; }
    public bool Boost { get; private set; }
    public bool Fire { get; private set; }


    void Start()
    {
        webcam = new VideoCapture();
        webcam.FlipHorizontal = true;
        CvInvoke.CheckLibraryLoaded();

        range = AccelerateTr - BrakeTr;

        if (rotationBufferSize % 2 == 0)
            rotations = new InputBuffer<float>(rotationBufferSize);
        else
            rotations = new InputBuffer<float>(rotationBufferSize + 1);

        positions = new InputBuffer<Vector3>(positionBufferSize);

        leftMarkerMin = MakeHsv(leftHsvMin);
        rightMarkerMin = MakeHsv(rightHsvMin);
        leftMarkerMax = MakeHsv(leftHsvMax);
        rightMarkerMax = MakeHsv(rightHsvMax);

        webcam.ImageGrabbed += GrabHandler;
    }

    private void LateUpdate()
    {
        if (webcam.IsOpened) webcam.Grab();
    }

    private void GrabHandler(object sender, EventArgs e)
    {
        if (!webcam.IsOpened) return;

        imgBgr = new Mat();
        webcam.Retrieve(imgBgr);
        imgIn = imgBgr.Clone();
        leftMarkerMin = MakeHsv(leftHsvMin);
        rightMarkerMin = MakeHsv(rightHsvMin);
        leftMarkerMax = MakeHsv(leftHsvMax);
        rightMarkerMax = MakeHsv(rightHsvMax);
        CvInvoke.CvtColor(imgIn, imgIn, ColorConversion.Bgr2Hsv);
        CvInvoke.GaussianBlur(imgIn, imgIn, new Size(5, 5), 0);

        var rectangle = GetColorRectangle(leftMarkerMin, leftMarkerMax, 0);
        var rectangle2 = GetColorRectangle(rightMarkerMin, rightMarkerMax, 1);

        PointF cl, cr;
        if (rectangle.HasValue && rectangle2.HasValue)
        {
            cl = rectangle.Value.Center;
            cr = rectangle2.Value.Center;

            // Calculate new angles average
            float angle = Mathf.Atan2(cr.Y - cl.Y, cr.X - cl.X) * Mathf.Rad2Deg;
            rotations.PushBack(angle);
            float angleAverage = 0.0f;
            foreach (var f in rotations.data)
                angleAverage += f;
            angleAverage /= rotations.curLength;
            SteeringAngle = angleAverage / 90;

            bool hopping = SteeringAngle > 1 || SteeringAngle < -1;
            HopPressed = hopping && !HopHeld;
            HopHeld = hopping;

            // Calculate new delta-positions average
            Vector3 currentCenter = (new Vector2(cl.X, cl.Y) + new Vector2(cr.X, cr.Y)) / 2;
            positions.PushBack(currentCenter);

            PointF[] array1 = rectangle.Value.GetVertices();
            PointF[] array2 = rectangle2.Value.GetVertices();
            PointF[] newArray = new PointF[array1.Length + array2.Length];
            Array.Copy(array1, newArray, array1.Length);
            Array.Copy(array2, 0, newArray, array1.Length, array2.Length);
            var boundRec = CvInvoke.MinAreaRect(newArray);

            var recSize = boundRec.Size.Height + boundRec.Size.Width;
            var pedal = recSize - BrakeTr;

            Brake = pedal < 0;

            Accelerate = pedal / range;

            //DrawPointsFRectangle(boundRec.GetVertices(), imgBgr);
            //CvInvoke.Imshow("azeCam", imgBgr);
            //CvInvoke.WaitKey(24);
        }

        else
        {
            Accelerate = 0;
            Brake = false;
            HopHeld = false;
            HopPressed = false;
        }
    }

    void DrawPointsFRectangle(PointF[] boundRecPoints, Mat output)
    {
        // Draw Bounding Rectangle from the first 4 points of "boundRecPoints" onto "output"
        CvInvoke.Line(output, new Point((int)boundRecPoints[0].X, (int)boundRecPoints[0].Y), new Point((int)boundRecPoints[1].X, (int)boundRecPoints[1].Y),
            new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
        CvInvoke.Line(output, new Point((int)boundRecPoints[1].X, (int)boundRecPoints[1].Y), new Point((int)boundRecPoints[2].X, (int)boundRecPoints[2].Y),
            new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
        CvInvoke.Line(output, new Point((int)boundRecPoints[2].X, (int)boundRecPoints[2].Y), new Point((int)boundRecPoints[3].X, (int)boundRecPoints[3].Y),
            new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
        CvInvoke.Line(output, new Point((int)boundRecPoints[3].X, (int)boundRecPoints[3].Y), new Point((int)boundRecPoints[0].X, (int)boundRecPoints[0].Y),
            new MCvScalar(100, 0, 0), 3, (LineType)8, 0);
    }

    private RotatedRect? GetColorRectangle(Hsv min, Hsv max, int index)
    {
        var orangeMaybe = imgIn.ToImage<Hsv, byte>().InRange(min, max);

        int operationSize = 1;
        var structuringElement = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(2 * operationSize + 1, 2 * operationSize + 1),
            new Point(operationSize, operationSize));

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
        webcam?.Dispose();
        CvInvoke.DestroyAllWindows();
    }
}