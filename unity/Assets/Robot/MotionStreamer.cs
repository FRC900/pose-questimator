/* Unity-based example for publishing Quest headset data to FRC-compatible Network Tables */
/* Juan Chong - 2024 */
/* test commit */
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnityRoboticsDemo;
using Oculus.Interaction.Samples;
using QuestMsg = RosMessageTypes.UnityRoboticsDemo.QuestPoseStampedMsg;
using RosColor = RosMessageTypes.UnityRoboticsDemo.UnityColorMsg;
using System;


/* Extend Vector3 with a ToArray() function */
public static class VectorExtensions
{
    public static float[] ToArray(this Vector3 vector)
    {
        return new float[] { vector.x, vector.y, vector.z };
    }
}

/* Extend Quaternion with a ToArray() function */
public static class QuaternionExtensions
{
    public static float[] ToArray(this Quaternion quaternion)
    {
        return new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w };
    }
}

public class MotionStreamer : MonoBehaviour
{
    /* Initialize local variables */
    public int frameIndex; // Local variable to store the headset frame index
    public double timeStamp; // Local variable to store the headset timestamp
    public Vector3 position; // Local variable to store the headset position in 3D space
    public Quaternion rotation; // Local variable to store the headset rotation in quaternion form
    public Vector3 eulerAngles; // Local variable to store the headset rotation in Euler angles
    public OVRCameraRig cameraRig;
    //public Nt4Source frcDataSink = null;
    private long command = 0;
    ROSConnection ros;
    public string topicName = "pos_rot";

    // Publish the cube's position and rotation every N seconds
    public float publishMessageFrequency = 0.0f;

    // Used to determine how much time has elapsed since the last message was published
    private float timeElapsed;
    [SerializeField] public Transform vrCamera; // The VR camera transform
    [SerializeField] public Transform vrCameraRoot; // The root of the camera transform
    [SerializeField] public Transform resetTransform; // The desired position & rotation (look direction) for your player

    void Start()
    {
        UnityEngine.Debug.Log("[MotionStreamer] Attempting to connect to the RoboRIO at TEST TEST TEST");

        UnityEngine.Debug.Log("RosPub Starting");

        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<RosColor>("color", zeroRobot);
        ros.RegisterPublisher<QuestMsg>(topicName);
        Debug.Log("HELLO I AM ALIVE");
        RecenterPlayer();
    }

    void LateUpdate()
    {
        PublishFrameData();
        //UnityEngine.Debug.Log("Motion ROS RosPub mpting to connect to the RoboRIO at TEST TEST TEST");
    }

    void zeroRobot(RosColor colorMessage)
    {
        UnityEngine.Debug.Log("[MotionStreamer] Zeroing robot");
        RecenterPlayer();
    }

    // Publish the Quest pose data to Network Tables
    private void PublishFrameData()
    {
        frameIndex = UnityEngine.Time.frameCount;
        timeStamp = UnityEngine.Time.time;
        position = cameraRig.centerEyeAnchor.position;
        rotation = cameraRig.centerEyeAnchor.rotation;
        eulerAngles = cameraRig.centerEyeAnchor.eulerAngles;
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            QuestPoseStampedMsg q3pose = new QuestPoseStampedMsg(
                frameIndex,
                timeStamp,
                position.x,
                position.y,
                position.z,
                rotation.x,
                rotation.y,
                rotation.z,
                rotation.w,
                eulerAngles.x,
                eulerAngles.y,
                eulerAngles.z

            );

            // Finally send the message to server_endpoint.py running in ROS
            ros.Publish(topicName, q3pose);

            timeElapsed = 0;
        }
        //UnityEngine.Debug.Log("Frame Index: " + frameIndex);
        //UnityEngine.Debug.Log("Timestamp: " + timeStamp);
        //UnityEngine.Debug.Log("Position: " + position);
        //UnityEngine.Debug.Log("Rotation: " + rotation);
        //UnityEngine.Debug.Log("Euler Angles: " + eulerAngles);

        //frcDataSink.PublishValue("/oculus/frameCount", frameIndex);
        //frcDataSink.PublishValue("/oculus/timestamp", timeStamp);
        //frcDataSink.PublishValue("/oculus/position", position.ToArray());
        //frcDataSink.PublishValue("/oculus/quaternion", rotation.ToArray());
        //frcDataSink.PublishValue("/oculus/eulerAngles", eulerAngles.ToArray());
    }

    // Process commands from the robot
    private void ProcessCommands()
    {
        //command = frcDataSink.GetLong("/oculus/mosi");
        //switch (command)
        //{
        //    case 1:
        //        RecenterPlayer();
        //        UnityEngine.Debug.Log("[MotionStreamer] Processed a heading reset request.");
        //        frcDataSink.PublishValue("/oculus/miso", 99);
        //        break;
        //    default:
        //        frcDataSink.PublishValue("/oculus/miso", 0);
        //        break;
        //}
    }

    // Clean up if the app crashes or is stopped
    void OnApplicationQuit()
    {

    }

    // Transform the HMD's rotation to virtually "zero" the robot position. Similar result as long-pressing the Oculus button.
    void RecenterPlayer()
    {
        float rotationAngleY = vrCamera.rotation.eulerAngles.y - resetTransform.rotation.eulerAngles.y;

        vrCameraRoot.transform.Rotate(0, -rotationAngleY, 0);

        Vector3 distanceDiff = resetTransform.position - vrCamera.position;
        vrCameraRoot.transform.position += distanceDiff;

    }
}