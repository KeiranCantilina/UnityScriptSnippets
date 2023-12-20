using System;
public class IMU_DataPacket
{
    public double rX;
    public double rY;
    public double rZ;
    public static IMU_DataPacket ParseDataPacket(byte[] data)
    {
        IMU_DataPacket IMU_Data = new IMU_DataPacket();
        IMU_Data.rX = BitConverter.ToDouble(data, 0);
        IMU_Data.rY = BitConverter.ToDouble(data, 8);
        IMU_Data.rZ = BitConverter.ToSingle(data, 16);

        return IMU_Data;
    }
    public override string ToString()
    {
        string rotX, rotY, rotZ;
       
        rotX = string.Format("{0:0.00} AfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfA'A?W", rX);
        rotY = string.Format("{0:0.00} AfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfA'A?W", rY);
        rotZ = string.Format("{0:0.00} AfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfAfA'A?W", rZ);

        return string.Format("rX: {0}, rY: {1}, rZ: {2}", rotX, rotY, rotZ);
    }
}