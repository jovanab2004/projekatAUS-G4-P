using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
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
            ushort adjustedAddress = mrcp.StartAddress;
            mdbRequest[8] = (byte)(adjustedAddress >> 8);
            mdbRequest[9] = (byte)adjustedAddress;
            mdbRequest[10] = (byte)(mrcp.Quantity >> 8);
            mdbRequest[11] = (byte)mrcp.Quantity;

            return mdbRequest;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters mrcp = CommandParameters as ModbusReadCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] > 0x80) { HandeException(response[8]); }

            int startIndex = 9;
            for (int i = 0; i < mrcp.Quantity; i++)
            {
                ushort value = (ushort)((response[startIndex + i * 2] << 8) | response[startIndex + i * 2 + 1]);
                ushort address = (ushort)(mrcp.StartAddress + i);
                result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address), value);
            }
            return result;
        }
    }
}