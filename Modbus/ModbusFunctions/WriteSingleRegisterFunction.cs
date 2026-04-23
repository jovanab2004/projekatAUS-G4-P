using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters mwcp = CommandParameters as ModbusWriteCommandParameters;
            byte[] mdbRequest = new byte[12];

            // MBAP Header
            mdbRequest[0] = (byte)(mwcp.TransactionId >> 8);
            mdbRequest[1] = (byte)mwcp.TransactionId;
            mdbRequest[2] = 0;
            mdbRequest[3] = 0;
            mdbRequest[4] = 0;
            mdbRequest[5] = 6;
            mdbRequest[6] = mwcp.UnitId;

            // PDU
            mdbRequest[7] = mwcp.FunctionCode; // 06
            ushort adjustedAddress = mwcp.OutputAddress;
            mdbRequest[8] = (byte)(adjustedAddress >> 8);
            mdbRequest[9] = (byte)adjustedAddress;

            // Vrednost registra (direktno mwcp.Value)
            mdbRequest[10] = (byte)(mwcp.Value >> 8);
            mdbRequest[11] = (byte)mwcp.Value;

            return mdbRequest;
        }

        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters mwcp = CommandParameters as ModbusWriteCommandParameters;
            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] > 0x80) { HandeException(response[8]); }

            // Potvrda upisane vrednosti iz odgovora
            ushort confirmedValue = (ushort)((response[10] << 8) | response[11]);

            result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, mwcp.OutputAddress), confirmedValue);
            return result;
        }
    }
}