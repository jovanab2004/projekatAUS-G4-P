using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters mwcp = CommandParameters as ModbusWriteCommandParameters;
            byte[] mdbRequest = new byte[12];

            mdbRequest[0] = (byte)(mwcp.TransactionId >> 8);
            mdbRequest[1] = (byte)mwcp.TransactionId;
            mdbRequest[2] = 0;
            mdbRequest[3] = 0;
            mdbRequest[4] = 0;
            mdbRequest[5] = 6;
            mdbRequest[6] = mwcp.UnitId;

            mdbRequest[7] = mwcp.FunctionCode;
            ushort adjustedAddress = mwcp.OutputAddress;
            mdbRequest[8] = (byte)(adjustedAddress >> 8);
            mdbRequest[9] = (byte)adjustedAddress;

            ushort coilValue = (ushort)(mwcp.Value != 0 ? 0xFF00 : 0x0000);
            mdbRequest[10] = (byte)(coilValue >> 8);
            mdbRequest[11] = (byte)coilValue;

            return mdbRequest;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters mwcp = CommandParameters as ModbusWriteCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] > 0x80) { HandeException(response[8]); }

            ushort confirmedValue = (ushort)((response[10] << 8) | response[11]);
            ushort displayValue = (ushort)(confirmedValue == 0xFF00 ? 1 : 0);

            result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, mwcp.OutputAddress), displayValue);
            return result;
        }
    }
}