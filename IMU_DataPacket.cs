using System;
public class IMU_DataPacket
{
    public float rX;
    public float rY;
    public float rZ;
    public float rW;
    public static IMU_DataPacket ParseDataPacket(byte[] data)

    {
        IMU_DataPacket IMU_Data = new IMU_DataPacket();
        IMU_Data.rX = (float) BitConverter.ToDouble(data, 0);
        IMU_Data.rY = (float) BitConverter.ToDouble(data, 8);
        IMU_Data.rZ = (float) BitConverter.ToDouble(data, 16);
        IMU_Data.rW = (float) BitConverter.ToDouble(data, 24);

        return IMU_Data;
    }
    public override string ToString()
    {
        string qX, qY, qZ, qW;
       
        qX = string.Format("{0:0.00} AfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfA'A?W", rX);
        qY = string.Format("{0:0.00} AfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfA'A?W", rY);
        qZ = string.Format("{0:0.00} AfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfA'A?W", rZ);
        qW = string.Format("{0:0.00} AfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfA'A?W", rW);

        return string.Format("qX: {0}, qY: {1}, qZ: {2}, qW: {3}", qX, qY, qZ, qW);
    }
}