using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    public class ReadCoilsFunction : ModbusFunction
    {
        public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters mrcp = CommandParameters as ModbusReadCommandParameters;
            byte[] mdbRequest = new byte[12];

            mdbRequest[0] = (byte)(mrcp.TransactionId >> 8);
            mdbRequest[1] = (byte)mrcp.TransactionId;
            mdbRequest[2] = 0; 
            mdbRequest[3] = 0;
            mdbRequest[4] = 0; 
            mdbRequest[5] = 6; 
            mdbRequest[6] = mrcp.UnitId; 

            mdbRequest[7] = mrcp.FunctionCode; 

            mdbRequest[8] = (byte)(mrcp.StartAddress >> 8);
            mdbRequest[9] = (byte)mrcp.StartAddress;

            mdbRequest[10] = (byte)(mrcp.Quantity >> 8);
            mdbRequest[11] = (byte)mrcp.Quantity;

            return mdbRequest;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters mrcp = CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] > 0x80)
            {
                HandeException(response[8]);
            }

            int byteCount = response[8];

            for (int i = 0; i < mrcp.Quantity; i++)
            {
                int byteOffset = i / 8;
                int bitOffset = i % 8;

                byte currentByte = response[9 + byteOffset];
                ushort value = (ushort)((currentByte >> bitOffset) & 1);

                ushort address = (ushort)(mrcp.StartAddress + i);
                result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, address), value);
            }

            return result;
        }
    }
}