﻿//// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Library;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AircraftTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IAircraft _Aircraft;

        [TestInitialize]
        public void TestInitialise()
        {
            _Aircraft = Factory.Resolve<IAircraft>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Nothing at the moment but I do in-test resets so best to have one of these, just in case
        }
        #endregion

        #region Constructor and properties
        [TestMethod]
        public void Aircraft_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.AreEqual(0, _Aircraft.AirPressureInHgChanged);
            Assert.AreEqual(0, _Aircraft.AltitudeChanged);
            Assert.AreEqual(0, _Aircraft.AltitudeTypeChanged);
            Assert.AreEqual(0, _Aircraft.CallsignChanged);
            Assert.AreEqual(0, _Aircraft.CallsignIsSuspectChanged);
            Assert.AreEqual(0, _Aircraft.ConstructionNumberChanged);
            Assert.AreEqual(0, _Aircraft.GeometricAltitudeChanged);
            Assert.AreEqual(0, _Aircraft.CountMessagesReceivedChanged);
            Assert.AreEqual(0, _Aircraft.DestinationChanged);
            Assert.AreEqual(0, _Aircraft.DestinationAirportCodeChanged);
            Assert.AreEqual(0, _Aircraft.EmergencyChanged);
            Assert.AreEqual(0, _Aircraft.EnginePlacementChanged);
            Assert.AreEqual(0, _Aircraft.EngineTypeChanged);
            Assert.AreEqual(0, _Aircraft.FirstSeenChanged);
            Assert.AreEqual(0, _Aircraft.FlightsCountChanged);
            Assert.AreEqual(0, _Aircraft.GroundSpeedChanged);
            Assert.AreEqual(0, _Aircraft.Icao24Changed);
            Assert.AreEqual(0, _Aircraft.Icao24CountryChanged);
            Assert.AreEqual(0, _Aircraft.Icao24InvalidChanged);
            Assert.AreEqual(0, _Aircraft.IdentActiveChanged);
            Assert.AreEqual(0, _Aircraft.IsCharterFlightChanged);
            Assert.AreEqual(0, _Aircraft.IsInterestingChanged);
            Assert.AreEqual(0, _Aircraft.IsMilitaryChanged);
            Assert.AreEqual(0, _Aircraft.IsPositioningFlightChanged);
            Assert.AreEqual(0, _Aircraft.IsTisbChanged);
            Assert.AreEqual(0, _Aircraft.LatitudeChanged);
            Assert.AreEqual(0, _Aircraft.LongitudeChanged);
            Assert.AreEqual(0, _Aircraft.ManufacturerChanged);
            Assert.AreEqual(0, _Aircraft.ModelChanged);
            Assert.AreEqual(0, _Aircraft.NumberOfEnginesChanged);
            Assert.AreEqual(0, _Aircraft.OnGroundChanged);
            Assert.AreEqual(0, _Aircraft.OperatorChanged);
            Assert.AreEqual(0, _Aircraft.OperatorIcaoChanged);
            Assert.AreEqual(0, _Aircraft.OriginChanged);
            Assert.AreEqual(0, _Aircraft.OriginAirportCodeChanged);
            Assert.AreEqual(0, _Aircraft.PictureFileNameChanged);
            Assert.AreEqual(0, _Aircraft.PictureHeightChanged);
            Assert.AreEqual(0, _Aircraft.PictureWidthChanged);
            Assert.AreEqual(0, _Aircraft.PositionIsMlatChanged);
            Assert.AreEqual(0, _Aircraft.PositionReceiverIdChanged);
            Assert.AreEqual(0, _Aircraft.PositionTimeChanged);
            Assert.AreEqual(0, _Aircraft.ReceiverIdChanged);
            Assert.AreEqual(0, _Aircraft.RegistrationChanged);
            Assert.AreEqual(0, _Aircraft.SignalLevelChanged);
            Assert.AreEqual(0, _Aircraft.SpeciesChanged);
            Assert.AreEqual(0, _Aircraft.SpeedTypeChanged);
            Assert.AreEqual(0, _Aircraft.SquawkChanged);
            Assert.AreEqual(0, _Aircraft.TargetAltitudeChanged);
            Assert.AreEqual(0, _Aircraft.TargetTrackChanged);
            Assert.AreEqual(0, _Aircraft.TrackChanged);
            Assert.AreEqual(0, _Aircraft.TrackIsHeadingChanged);
            Assert.AreEqual(0, _Aircraft.TransponderTypeChanged);
            Assert.AreEqual(0, _Aircraft.TypeChanged);
            Assert.AreEqual(0, _Aircraft.UserNotesChanged);
            Assert.AreEqual(0, _Aircraft.UserTagChanged);
            Assert.AreEqual(0, _Aircraft.VerticalRateChanged);
            Assert.AreEqual(0, _Aircraft.VerticalRateTypeChanged);
            Assert.AreEqual(0, _Aircraft.WakeTurbulenceCategoryChanged);
            Assert.AreEqual(0, _Aircraft.YearBuiltChanged);

            Assert.AreEqual(0, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.Stopovers.Count);

            TestUtilities.TestProperty(_Aircraft, r => r.AirPressureInHg, null, 1.2F);
            TestUtilities.TestProperty(_Aircraft, r => r.AirPressureLookedUpUtc, null, DateTime.UtcNow);
            TestUtilities.TestProperty(_Aircraft, r => r.Altitude, null, 1);
            TestUtilities.TestProperty(_Aircraft, r => r.AltitudeType, AltitudeType.Barometric, AltitudeType.Geometric);
            TestUtilities.TestProperty(_Aircraft, r => r.Callsign, null, "ABC123");
            TestUtilities.TestProperty(_Aircraft, r => r.CallsignIsSuspect, false);
            TestUtilities.TestProperty(_Aircraft, r => r.GeometricAltitude, null, 100);
            TestUtilities.TestProperty(_Aircraft, r => r.ConstructionNumber, null, "GB");
            TestUtilities.TestProperty(_Aircraft, r => r.CountMessagesReceived, 0L, 12L);
            TestUtilities.TestProperty(_Aircraft, r => r.DataVersion, 0L, 12023L);
            TestUtilities.TestProperty(_Aircraft, r => r.Destination, null, "XYZ");
            TestUtilities.TestProperty(_Aircraft, r => r.DestinationAirportCode, null, "XYZ");
            TestUtilities.TestProperty(_Aircraft, r => r.Emergency, null, false);
            TestUtilities.TestProperty(_Aircraft, r => r.EnginePlacement, EnginePlacement.Unknown, EnginePlacement.AftMounted);
            TestUtilities.TestProperty(_Aircraft, r => r.EngineType, EngineType.None, EngineType.Jet);
            TestUtilities.TestProperty(_Aircraft, r => r.FirstCoordinateChanged, 0L, 123L);
            TestUtilities.TestProperty(_Aircraft, r => r.FirstSeen, DateTime.MinValue, DateTime.Now);
            TestUtilities.TestProperty(_Aircraft, r => r.FlightsCount, 0, 120);
            TestUtilities.TestProperty(_Aircraft, r => r.GroundSpeed, null, 5.4f);
            TestUtilities.TestProperty(_Aircraft, r => r.Icao24, null, "ABCDEF");
            TestUtilities.TestProperty(_Aircraft, r => r.Icao24Country, null, "HG");
            TestUtilities.TestProperty(_Aircraft, r => r.Icao24Invalid, false);
            TestUtilities.TestProperty(_Aircraft, r => r.IsCharterFlight, false);
            TestUtilities.TestProperty(_Aircraft, r => r.IsInteresting, false);
            TestUtilities.TestProperty(_Aircraft, r => r.IdentActive, null, true);
            TestUtilities.TestProperty(_Aircraft, r => r.IsMilitary, false);
            TestUtilities.TestProperty(_Aircraft, r => r.IsPositioningFlight, false);
            TestUtilities.TestProperty(_Aircraft, r => r.IsTisb, false);
            TestUtilities.TestProperty(_Aircraft, r => r.IsTransmittingTrack, false);
            TestUtilities.TestProperty(_Aircraft, r => r.LastCoordinateChanged, 0L, 123L);
            TestUtilities.TestProperty(_Aircraft, r => r.LastUpdate, DateTime.MinValue, DateTime.Now);
            TestUtilities.TestProperty(_Aircraft, r => r.LastModeSUpdate, DateTime.MinValue, DateTime.Now.AddSeconds(1));
            TestUtilities.TestProperty(_Aircraft, r => r.LastSatcomUpdate, DateTime.MinValue, DateTime.Now.AddSeconds(2));
            TestUtilities.TestProperty(_Aircraft, r => r.LatestCoordinateTime, DateTime.MinValue, DateTime.Now);
            TestUtilities.TestProperty(_Aircraft, r => r.Latitude, null, 8.3213);
            TestUtilities.TestProperty(_Aircraft, r => r.Longitude, null, 12.3231);
            TestUtilities.TestProperty(_Aircraft, r => r.Manufacturer, null, "FG");
            TestUtilities.TestProperty(_Aircraft, r => r.Model, null, "UI");
            TestUtilities.TestProperty(_Aircraft, r => r.NumberOfEngines, null, "R2D2");
            TestUtilities.TestProperty(_Aircraft, r => r.OnGround, null, true);
            TestUtilities.TestProperty(_Aircraft, r => r.Operator, null, "HJ");
            TestUtilities.TestProperty(_Aircraft, r => r.OperatorIcao, null, "YUI");
            TestUtilities.TestProperty(_Aircraft, r => r.Origin, null, "TG");
            TestUtilities.TestProperty(_Aircraft, r => r.OriginAirportCode, null, "TG");
            TestUtilities.TestProperty(_Aircraft, r => r.PictureFileName, null, "VG");
            TestUtilities.TestProperty(_Aircraft, r => r.PictureHeight, 0, 1024);
            TestUtilities.TestProperty(_Aircraft, r => r.PictureWidth, 0, 2048);
            TestUtilities.TestProperty(_Aircraft, r => r.PositionIsMlat, null, true);
            TestUtilities.TestProperty(_Aircraft, r => r.PositionReceiverId, null, 1);
            TestUtilities.TestProperty(_Aircraft, r => r.PositionTime, null, DateTime.Now);
            TestUtilities.TestProperty(_Aircraft, r => r.ReceiverId, 0, 1234);
            TestUtilities.TestProperty(_Aircraft, r => r.Registration, null, "JS");
            TestUtilities.TestProperty(_Aircraft, r => r.SignalLevel, null, 123);
            TestUtilities.TestProperty(_Aircraft, r => r.Species, Species.None, Species.Seaplane);
            TestUtilities.TestProperty(_Aircraft, r => r.SpeedType, SpeedType.GroundSpeed, SpeedType.TrueAirSpeed);
            TestUtilities.TestProperty(_Aircraft, r => r.Squawk, null, 7654);
            TestUtilities.TestProperty(_Aircraft, r => r.StopoversChanged, 0L, 123L);
            TestUtilities.TestProperty(_Aircraft, r => r.TargetAltitude, null, 1);
            TestUtilities.TestProperty(_Aircraft, r => r.TargetTrack, null, 1.234F);
            TestUtilities.TestProperty(_Aircraft, r => r.Track, null, 12.345F);
            TestUtilities.TestProperty(_Aircraft, r => r.TrackIsHeading, false);
            TestUtilities.TestProperty(_Aircraft, r => r.TransponderType, TransponderType.Unknown, TransponderType.Adsb2);
            TestUtilities.TestProperty(_Aircraft, r => r.Type, null, "9JH");
            TestUtilities.TestProperty(_Aircraft, r => r.UserNotes, null, "ABC");
            TestUtilities.TestProperty(_Aircraft, r => r.UserTag, null, "ABC");
            TestUtilities.TestProperty(_Aircraft, r => r.UniqueId, 0, 12);
            TestUtilities.TestProperty(_Aircraft, r => r.VerticalRate, null, -120);
            TestUtilities.TestProperty(_Aircraft, r => r.VerticalRateType, AltitudeType.Barometric, AltitudeType.Geometric);
            TestUtilities.TestProperty(_Aircraft, r => r.WakeTurbulenceCategory, WakeTurbulenceCategory.None, WakeTurbulenceCategory.Heavy);
            TestUtilities.TestProperty(_Aircraft, r => r.YearBuilt, null, "Abc123");
        }

        [TestMethod]
        public void Aircraft_PropertyChanged_Properties_Are_Correctly_Updated_When_Property_Changes()
        {
            // The coordinate lists are not auto-updating, there is a function to add coordinates that takes care of things like
            // dataversions. The others have dedicated tests for them.
            var ignoreProperties = new string[] {
                nameof(IAircraft.FirstCoordinateChanged),
                nameof(IAircraft.LastCoordinateChanged),
                nameof(IAircraft.StopoversChanged),
                nameof(IAircraft.StopoverAirportCodesChanged),
            };

            var changedProperties = typeof(IAircraft).GetProperties().Where(p => !ignoreProperties.Contains(p.Name) && p.Name.EndsWith("Changed")).ToList();

            foreach(var changedProperty in changedProperties) {
                var valueProperty = typeof(IAircraft).GetProperty(changedProperty.Name.Substring(0, changedProperty.Name.Length - 7));
                if(valueProperty == null) continue;

                // When we change the "value" property it should automatically copy the value of DataVersion to the "changed" property.
                // E.G. if we assign 1 to DataVersion and then assign "A" to the Registration property then RegistrationChanged should be
                // set to 1 automatically. No other changedProperty should be modified.

                TestCleanup();
                TestInitialise();

                _Aircraft.DataVersion = 99;

                var value = AircraftTestHelper.GenerateAircraftPropertyValue(valueProperty.PropertyType);
                valueProperty.SetValue(_Aircraft, value, null);

                Assert.AreEqual(99, (long)changedProperty.GetValue(_Aircraft, null), "Setting {0} did not update {1}", valueProperty.Name, changedProperty.Name);

                foreach(var otherChangedProperty in changedProperties.Where(p => p != changedProperty)) {
                    Assert.AreEqual(0, (long)otherChangedProperty.GetValue(_Aircraft, null), "Setting {0} updated {1} as well as {2}", valueProperty.Name, otherChangedProperty.Name, changedProperty.Name);
                }
            }
        }

        [TestMethod]
        public void Aircraft_PropertyChanged_Modifying_Stopovers_Updates_StopoversChanged()
        {
            _Aircraft.DataVersion = 1;
            _Aircraft.Stopovers.Add("Item 1");
            Assert.AreEqual(1, _Aircraft.StopoversChanged);

            _Aircraft.DataVersion = 200;
            _Aircraft.Stopovers.Add("Item 2");
            Assert.AreEqual(200, _Aircraft.StopoversChanged);

            _Aircraft.DataVersion = 9010;
            _Aircraft.Stopovers.Clear();
            Assert.AreEqual(9010, _Aircraft.StopoversChanged);
        }

        [TestMethod]
        public void Aircraft_PropertyChanged_Modifying_StopoverAirportCodes_Updates_StopoverAirportCodesChanged()
        {
            _Aircraft.DataVersion = 1;
            _Aircraft.StopoverAirportCodes.Add("Item 1");
            Assert.AreEqual(1, _Aircraft.StopoverAirportCodesChanged);

            _Aircraft.DataVersion = 200;
            _Aircraft.StopoverAirportCodes.Add("Item 2");
            Assert.AreEqual(200, _Aircraft.StopoverAirportCodesChanged);

            _Aircraft.DataVersion = 9010;
            _Aircraft.StopoverAirportCodes.Clear();
            Assert.AreEqual(9010, _Aircraft.StopoverAirportCodesChanged);
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "IcaoCompliantRegistration$")]
        public void Aircraft_IcaoCompliantRegistration_Correctly_Transforms_Registration_To_Comply_With_ICAO_Rules()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = Factory.Resolve<IAircraft>();
            aircraft.Registration = worksheet.EString("Registration");

            Assert.AreEqual(worksheet.EString("IcaoCompliantRegistration"), aircraft.IcaoCompliantRegistration);
            Assert.AreEqual(worksheet.EString("Registration"), aircraft.Registration);
        }

        [TestMethod]
        public void Aircraft_IcaoCompliantRegistration_Changes_When_Registration_Changes()
        {
            var aircraft = Factory.Resolve<IAircraft>();

            aircraft.DataVersion = 1;
            aircraft.Registration = "ABC";
            Assert.AreEqual("ABC", aircraft.IcaoCompliantRegistration);

            aircraft.DataVersion = 2;
            aircraft.Registration = "DEF";
            Assert.AreEqual("DEF", aircraft.IcaoCompliantRegistration);
        }

        [TestMethod]
        public void Aircraft_IcaoCompliantRegistration_Can_Be_Reset_To_Null()
        {
            var aircraft = Factory.Resolve<IAircraft>();

            aircraft.DataVersion = 1;
            aircraft.Registration = "ABC";
            Assert.AreEqual("ABC", aircraft.IcaoCompliantRegistration);

            aircraft.DataVersion = 2;
            aircraft.Registration = null;
            Assert.AreEqual(null, aircraft.IcaoCompliantRegistration);
        }
        #endregion

        #region Clone
        [TestMethod]
        public void Aircraft_Clone_Copies_Every_Property_Correctly()
        {
            Dictionary<string, object> values = new Dictionary<string,object>();

            int intValue = 1;
            float floatValue = 121.3414F;
            double doubleValue = 121.3414;
            long longValue = (long)int.MaxValue + 1L;
            DateTime dateTimeValue = new DateTime(2001, 1, 2);
            string stringValue = "";

            var allBoolPropertyNames = typeof(IAircraft).GetProperties().Where(r => r.PropertyType == typeof(bool) || r.PropertyType == typeof(bool?)).Select(r => r.Name).ToList();
            foreach(var trueBool in allBoolPropertyNames) {
                TestCleanup();
                TestInitialise();
                values.Clear();

                for(int setChangedValues = 0;setChangedValues < 2;++setChangedValues) {
                    // Set the properties of the object to clone using reflection to make sure
                    // we don't forget to update this test in the future...
                    foreach(var property in typeof(IAircraft).GetProperties()) {
                        if(property.Name == "DataVersion") continue;
                        bool isChangedValue = property.Name.EndsWith("Changed");
                        if(setChangedValues == 0 && isChangedValue) continue;
                        if(setChangedValues == 1 && !isChangedValue) continue;

                        ++intValue;
                        ++floatValue;
                        ++longValue;
                        dateTimeValue = dateTimeValue.AddDays(1);
                        ++_Aircraft.DataVersion;
                        stringValue = String.Format("A{0}", intValue);

                        if(!values.ContainsKey("DataVersion")) values.Add("DataVersion", _Aircraft.DataVersion);
                        else                                   values["DataVersion"] = _Aircraft.DataVersion;

                        object value = null;
                        switch(property.Name) {
                            case nameof(IAircraft.UniqueId):                        value = _Aircraft.UniqueId = intValue; break;
                            case nameof(IAircraft.Icao24):                          value = _Aircraft.Icao24 = stringValue; break;
                            case nameof(IAircraft.Icao24Changed):                   value = _Aircraft.Icao24Changed; break;
                            case nameof(IAircraft.Icao24Invalid):                   value = _Aircraft.Icao24Invalid = property.Name == trueBool; break;
                            case nameof(IAircraft.Icao24InvalidChanged):            value = _Aircraft.Icao24InvalidChanged; break;
                            case nameof(IAircraft.Callsign):                        value = _Aircraft.Callsign = stringValue; break;
                            case nameof(IAircraft.CallsignChanged):                 value = _Aircraft.CallsignChanged; break;
                            case nameof(IAircraft.CallsignIsSuspect):               value = _Aircraft.CallsignIsSuspect = property.Name == trueBool; break;
                            case nameof(IAircraft.CallsignIsSuspectChanged):        value = _Aircraft.CallsignIsSuspectChanged; break;
                            case nameof(IAircraft.CountMessagesReceived):           value = _Aircraft.CountMessagesReceived = longValue; break;
                            case nameof(IAircraft.CountMessagesReceivedChanged):    value = _Aircraft.CountMessagesReceivedChanged; break;
                            case nameof(IAircraft.Altitude):                        value = _Aircraft.Altitude = intValue; break;
                            case nameof(IAircraft.AltitudeChanged):                 value = _Aircraft.AltitudeChanged; break;
                            case nameof(IAircraft.AltitudeType):                    value = _Aircraft.AltitudeType = AltitudeType.Geometric; break;
                            case nameof(IAircraft.AltitudeTypeChanged):             value = _Aircraft.AltitudeTypeChanged; break;
                            case nameof(IAircraft.GroundSpeed):                     value = _Aircraft.GroundSpeed = intValue; break;
                            case nameof(IAircraft.GroundSpeedChanged):              value = _Aircraft.GroundSpeedChanged; break;
                            case nameof(IAircraft.Latitude):                        value = _Aircraft.Latitude = doubleValue; break;
                            case nameof(IAircraft.LatitudeChanged):                 value = _Aircraft.LatitudeChanged; break;
                            case nameof(IAircraft.Longitude):                       value = _Aircraft.Longitude = doubleValue; break;
                            case nameof(IAircraft.LongitudeChanged):                value = _Aircraft.LongitudeChanged; break;
                            case nameof(IAircraft.PositionIsMlat):                  value = _Aircraft.PositionIsMlat = property.Name == trueBool; break;
                            case nameof(IAircraft.PositionIsMlatChanged):           value = _Aircraft.PositionIsMlatChanged; break;
                            case nameof(IAircraft.PositionReceiverId):              value = _Aircraft.PositionReceiverId = intValue; break;
                            case nameof(IAircraft.PositionReceiverIdChanged):       value = _Aircraft.PositionReceiverIdChanged; break;
                            case nameof(IAircraft.PositionTime):                    value = _Aircraft.PositionTime = dateTimeValue; break;
                            case nameof(IAircraft.PositionTimeChanged):             value = _Aircraft.PositionTimeChanged; break;
                            case nameof(IAircraft.Track):                           value = _Aircraft.Track = floatValue; break;
                            case nameof(IAircraft.TrackChanged):                    value = _Aircraft.TrackChanged; break;
                            case nameof(IAircraft.IsTransmittingTrack):             value = _Aircraft.IsTransmittingTrack = property.Name == trueBool; break;
                            case nameof(IAircraft.VerticalRate):                    value = _Aircraft.VerticalRate = intValue; break;
                            case nameof(IAircraft.VerticalRateChanged):             value = _Aircraft.VerticalRateChanged; break;
                            case nameof(IAircraft.Squawk):                          value = _Aircraft.Squawk = intValue; break;
                            case nameof(IAircraft.SquawkChanged):                   value = _Aircraft.SquawkChanged; break;
                            case nameof(IAircraft.Emergency):                       value = _Aircraft.Emergency = property.Name == trueBool; break;
                            case nameof(IAircraft.EmergencyChanged):                value = _Aircraft.EmergencyChanged; break;
                            case nameof(IAircraft.Registration):                    value = _Aircraft.Registration = stringValue; break;
                            case nameof(IAircraft.RegistrationChanged):             value = _Aircraft.RegistrationChanged; break;
                            case nameof(IAircraft.FirstSeen):                       value = _Aircraft.FirstSeen = dateTimeValue; break;
                            case nameof(IAircraft.FirstSeenChanged):                value = _Aircraft.FirstSeenChanged; break;
                            case nameof(IAircraft.FlightsCount):                    value = _Aircraft.FlightsCount = intValue; break;
                            case nameof(IAircraft.FlightsCountChanged):             value = _Aircraft.FlightsCountChanged; break;
                            case nameof(IAircraft.LastUpdate):                      value = _Aircraft.LastUpdate = dateTimeValue; break;
                            case nameof(IAircraft.LastModeSUpdate):                 value = _Aircraft.LastModeSUpdate = dateTimeValue; break;
                            case nameof(IAircraft.LastSatcomUpdate):                value = _Aircraft.LastSatcomUpdate = dateTimeValue; break;
                            case nameof(IAircraft.Type):                            value = _Aircraft.Type = stringValue; break;
                            case nameof(IAircraft.TypeChanged):                     value = _Aircraft.TypeChanged; break;
                            case nameof(IAircraft.Manufacturer):                    value = _Aircraft.Manufacturer = stringValue; break;
                            case nameof(IAircraft.ManufacturerChanged):             value = _Aircraft.ManufacturerChanged; break;
                            case nameof(IAircraft.Model):                           value = _Aircraft.Model = stringValue; break;
                            case nameof(IAircraft.ModelChanged):                    value = _Aircraft.ModelChanged; break;
                            case nameof(IAircraft.ConstructionNumber):              value = _Aircraft.ConstructionNumber = stringValue; break;
                            case nameof(IAircraft.ConstructionNumberChanged):       value = _Aircraft.ConstructionNumberChanged; break;
                            case nameof(IAircraft.Origin):                          value = _Aircraft.Origin = stringValue; break;
                            case nameof(IAircraft.OriginChanged):                   value = _Aircraft.OriginChanged; break;
                            case nameof(IAircraft.OriginAirportCode):               value = _Aircraft.OriginAirportCode = stringValue; break;
                            case nameof(IAircraft.OriginAirportCodeChanged):        value = _Aircraft.OriginAirportCodeChanged; break;
                            case nameof(IAircraft.Destination):                     value = _Aircraft.Destination = stringValue; break;
                            case nameof(IAircraft.DestinationChanged):              value = _Aircraft.DestinationChanged; break;
                            case nameof(IAircraft.DestinationAirportCode):          value = _Aircraft.DestinationAirportCode = stringValue; break;
                            case nameof(IAircraft.DestinationAirportCodeChanged):   value = _Aircraft.DestinationAirportCodeChanged; break;
                            case nameof(IAircraft.SignalLevel):                     value = _Aircraft.SignalLevel = intValue; break;
                            case nameof(IAircraft.SignalLevelChanged):              value = _Aircraft.SignalLevelChanged; break;
                            case nameof(IAircraft.Stopovers):                       value = stringValue; _Aircraft.Stopovers.Add(stringValue); break;
                            case nameof(IAircraft.StopoversChanged):                value = _Aircraft.StopoversChanged = longValue; break;
                            case nameof(IAircraft.StopoverAirportCodes):            value = stringValue; _Aircraft.StopoverAirportCodes.Add(stringValue); break;
                            case nameof(IAircraft.StopoverAirportCodesChanged):     value = _Aircraft.StopoverAirportCodesChanged = longValue; break;
                            case nameof(IAircraft.Operator):                        value = _Aircraft.Operator = stringValue; break;
                            case nameof(IAircraft.OperatorChanged):                 value = _Aircraft.OperatorChanged; break;
                            case nameof(IAircraft.OperatorIcao):                    value = _Aircraft.OperatorIcao = stringValue; break;
                            case nameof(IAircraft.OperatorIcaoChanged):             value = _Aircraft.OperatorIcaoChanged; break;
                            case nameof(IAircraft.WakeTurbulenceCategory):          value = _Aircraft.WakeTurbulenceCategory = WakeTurbulenceCategory.Light; break;
                            case nameof(IAircraft.WakeTurbulenceCategoryChanged):   value = _Aircraft.WakeTurbulenceCategoryChanged; break;
                            case nameof(IAircraft.EnginePlacement):                 value = _Aircraft.EnginePlacement = EnginePlacement.AftMounted; break;
                            case nameof(IAircraft.EnginePlacementChanged):          value = _Aircraft.EnginePlacementChanged; break;
                            case nameof(IAircraft.EngineType):                      value = _Aircraft.EngineType = EngineType.Electric; break;
                            case nameof(IAircraft.EngineTypeChanged):               value = _Aircraft.EngineTypeChanged; break;
                            case nameof(IAircraft.NumberOfEngines):                 value = _Aircraft.NumberOfEngines = stringValue; break;
                            case nameof(IAircraft.NumberOfEnginesChanged):          value = _Aircraft.NumberOfEnginesChanged; break;
                            case nameof(IAircraft.Species):                         value = _Aircraft.Species = Species.Gyrocopter; break;
                            case nameof(IAircraft.SpeciesChanged):                  value = _Aircraft.SpeciesChanged; break;
                            case nameof(IAircraft.IsMilitary):                      value = _Aircraft.IsMilitary = property.Name == trueBool; break;
                            case nameof(IAircraft.IsMilitaryChanged):               value = _Aircraft.IsMilitaryChanged; break;
                            case nameof(IAircraft.Icao24Country):                   value = _Aircraft.Icao24Country = stringValue; break;
                            case nameof(IAircraft.Icao24CountryChanged):            value = _Aircraft.Icao24CountryChanged; break;
                            case nameof(IAircraft.PictureFileName):                 value = _Aircraft.PictureFileName = stringValue; break;
                            case nameof(IAircraft.PictureFileNameChanged):          value = _Aircraft.PictureFileNameChanged; break;
                            case nameof(IAircraft.FirstCoordinateChanged):          value = _Aircraft.FirstCoordinateChanged = longValue; break;
                            case nameof(IAircraft.LastCoordinateChanged):           value = _Aircraft.LastCoordinateChanged = longValue; break;
                            case nameof(IAircraft.LatestCoordinateTime):            value = _Aircraft.LatestCoordinateTime = dateTimeValue; break;
                            case nameof(IAircraft.FullCoordinates):                 _Aircraft.FullCoordinates.Add(new Coordinate(1, 2, 3F, 4F, 5F)); value = _Aircraft.FullCoordinates[0]; break;
                            case nameof(IAircraft.ShortCoordinates):                _Aircraft.ShortCoordinates.Add(new Coordinate(11, 12, 13F, 14F, 15F)); value = _Aircraft.ShortCoordinates[0]; break;
                            case nameof(IAircraft.IcaoCompliantRegistration):       value = _Aircraft.Registration; break;
                            case nameof(IAircraft.IsInteresting):                   value = _Aircraft.IsInteresting = property.Name == trueBool; break;
                            case nameof(IAircraft.IsInterestingChanged):            value = _Aircraft.IsInterestingChanged; break;
                            case nameof(IAircraft.OnGround):                        value = _Aircraft.OnGround = property.Name == trueBool; break;
                            case nameof(IAircraft.OnGroundChanged):                 value = _Aircraft.OnGroundChanged; break;
                            case nameof(IAircraft.SpeedType):                       value = _Aircraft.SpeedType = SpeedType.TrueAirSpeed; break;
                            case nameof(IAircraft.SpeedTypeChanged):                value = _Aircraft.SpeedTypeChanged; break;
                            case nameof(IAircraft.UserTag):                         value = _Aircraft.UserTag = stringValue; break;
                            case nameof(IAircraft.UserTagChanged):                  value = _Aircraft.UserTagChanged; break;
                            case nameof(IAircraft.UserNotes):                       value = _Aircraft.UserNotes = stringValue; break;
                            case nameof(IAircraft.UserNotesChanged):                value = _Aircraft.UserNotesChanged; break;
                            case nameof(IAircraft.ReceiverId):                      value = _Aircraft.ReceiverId = intValue; break;
                            case nameof(IAircraft.ReceiverIdChanged):               value = _Aircraft.ReceiverIdChanged; break;
                            case nameof(IAircraft.PictureWidth):                    value = _Aircraft.PictureWidth = intValue; break;
                            case nameof(IAircraft.PictureWidthChanged):             value = _Aircraft.PictureWidthChanged; break;
                            case nameof(IAircraft.PictureHeight):                   value = _Aircraft.PictureHeight = intValue; break;
                            case nameof(IAircraft.PictureHeightChanged):            value = _Aircraft.PictureHeightChanged; break;
                            case nameof(IAircraft.VerticalRateType):                value = _Aircraft.VerticalRateType = AltitudeType.Geometric; break;
                            case nameof(IAircraft.VerticalRateTypeChanged):         value = _Aircraft.VerticalRateTypeChanged; break;
                            case nameof(IAircraft.TrackIsHeading):                  value = _Aircraft.TrackIsHeading = property.Name == trueBool; break;
                            case nameof(IAircraft.TrackIsHeadingChanged):           value = _Aircraft.TrackIsHeadingChanged; break;
                            case nameof(IAircraft.TransponderType):                 value = _Aircraft.TransponderType = TransponderType.Adsb2; break;
                            case nameof(IAircraft.TransponderTypeChanged):          value = _Aircraft.TransponderTypeChanged; break;
                            case nameof(IAircraft.TargetAltitude):                  value = _Aircraft.TargetAltitude = intValue; break;
                            case nameof(IAircraft.TargetAltitudeChanged):           value = _Aircraft.TargetAltitudeChanged; break;
                            case nameof(IAircraft.TargetTrack):                     value = _Aircraft.TargetTrack = floatValue; break;
                            case nameof(IAircraft.TargetTrackChanged):              value = _Aircraft.TargetTrackChanged; break;
                            case nameof(IAircraft.IsTisb):                          value = _Aircraft.IsTisb = property.Name == trueBool; break;
                            case nameof(IAircraft.IsTisbChanged):                   value = _Aircraft.IsTisbChanged; break;
                            case nameof(IAircraft.YearBuilt):                       value = _Aircraft.YearBuilt = stringValue; break;
                            case nameof(IAircraft.YearBuiltChanged):                value = _Aircraft.YearBuiltChanged; break;
                            case nameof(IAircraft.GeometricAltitude):               value = _Aircraft.GeometricAltitude = intValue; break;
                            case nameof(IAircraft.GeometricAltitudeChanged):        value = _Aircraft.GeometricAltitudeChanged; break;
                            case nameof(IAircraft.AirPressureInHg):                 value = _Aircraft.AirPressureInHg = floatValue; break;
                            case nameof(IAircraft.AirPressureInHgChanged):          value = _Aircraft.AirPressureInHgChanged; break;
                            case nameof(IAircraft.AirPressureLookedUpUtc):          value = _Aircraft.AirPressureLookedUpUtc = dateTimeValue; break;
                            case nameof(IAircraft.IdentActive):                     value = _Aircraft.IdentActive = property.Name == trueBool; break;
                            case nameof(IAircraft.IdentActiveChanged):              value = _Aircraft.IdentActiveChanged; break;
                            case nameof(IAircraft.IsCharterFlight):                 value = _Aircraft.IsCharterFlight = property.Name == trueBool; break;
                            case nameof(IAircraft.IsCharterFlightChanged):          value = _Aircraft.IsCharterFlightChanged; break;
                            case nameof(IAircraft.IsPositioningFlight):             value = _Aircraft.IsPositioningFlight = property.Name == trueBool; break;
                            case nameof(IAircraft.IsPositioningFlightChanged):      value = _Aircraft.IsPositioningFlightChanged; break;
                            default:                                                throw new NotImplementedException();
                        }

                        Assert.IsNotNull(value, property.Name);
                        values.Add(property.Name, value);
                    }
                }

                var clone = (IAircraft)_Aircraft.Clone();

                foreach(var property in typeof(IAircraft).GetProperties()) {
                    var expected = values[property.Name];
                    switch(property.Name) {
                        case nameof(IAircraft.Stopovers):               Assert.AreEqual(expected, clone.Stopovers.First(), property.Name); break;
                        case nameof(IAircraft.StopoverAirportCodes):    Assert.AreEqual(expected, clone.StopoverAirportCodes.First(), property.Name); break;
                        case nameof(IAircraft.FullCoordinates):         Assert.AreEqual(expected, clone.FullCoordinates[0], property.Name); break;
                        case nameof(IAircraft.ShortCoordinates):        Assert.AreEqual(expected, clone.ShortCoordinates[0], property.Name); break;
                        default:
                            var actual = property.GetValue(clone, null);
                            Assert.AreEqual(expected, actual, property.Name); break;
                    }
                }
            }
        }
        #endregion

        #region ResetCoordinates
        [TestMethod]
        public void Aircraft_ResetCoordinates_Resets_Coordinate_Properties()
        {
            _Aircraft.LatestCoordinateTime = DateTime.Now;
            _Aircraft.FullCoordinates.Add(new Coordinate(1, 2, 3f, 4f, null));
            _Aircraft.ShortCoordinates.Add(new Coordinate(1, 2, 3f, 4f, null));
            _Aircraft.FirstCoordinateChanged = 88;
            _Aircraft.LastCoordinateChanged = 99;

            _Aircraft.ResetCoordinates();

            Assert.AreEqual(DateTime.MinValue, _Aircraft.LatestCoordinateTime);
            Assert.AreEqual(0, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(0, _Aircraft.LastCoordinateChanged);
        }
        #endregion

        #region UpdateCoordinates
        [TestMethod]
        public void Aircraft_UpdateCoordinates_Adds_Coordinate_To_Full_And_Short_Trails()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks + 1;
            _Aircraft.Latitude = 10;
            _Aircraft.Longitude = 20;
            _Aircraft.Track = 30;
            _Aircraft.Altitude = 20000;
            _Aircraft.GroundSpeed = 250;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);

            var fullCoordinate = _Aircraft.FullCoordinates[0];
            Assert.AreEqual(10f, fullCoordinate.Latitude);
            Assert.AreEqual(20f, fullCoordinate.Longitude);
            Assert.AreEqual(30f, fullCoordinate.Heading);
            Assert.AreEqual(20000, fullCoordinate.Altitude);
            Assert.AreEqual(250, fullCoordinate.GroundSpeed);
            Assert.AreEqual(now.Ticks + 1, fullCoordinate.DataVersion);
            Assert.AreEqual(now.Ticks, fullCoordinate.Tick);

            var shortCoordinate = _Aircraft.ShortCoordinates[0];
            Assert.AreEqual(10f, shortCoordinate.Latitude);
            Assert.AreEqual(20f, shortCoordinate.Longitude);
            Assert.AreEqual(30f, shortCoordinate.Heading);
            Assert.AreEqual(20000, shortCoordinate.Altitude);
            Assert.AreEqual(250, shortCoordinate.GroundSpeed);
            Assert.AreEqual(now.Ticks + 1, shortCoordinate.DataVersion);
            Assert.AreEqual(now.Ticks, shortCoordinate.Tick);
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Updates_Coordinate_Times()
        {
            var now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks + 1;
            _Aircraft.Latitude = 10;
            _Aircraft.Longitude = 20;
            _Aircraft.Track = 30;
            _Aircraft.UpdateCoordinates(now, 30);

            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now.Ticks + 1, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(now.Ticks + 1, _Aircraft.LastCoordinateChanged);
            Assert.AreEqual(now, _Aircraft.LatestCoordinateTime);

            var nowPlusOne = now.AddSeconds(1);
            _Aircraft.DataVersion = nowPlusOne.Ticks;
            _Aircraft.Latitude = 10.0001f;
            _Aircraft.UpdateCoordinates(nowPlusOne, 30);

            Assert.AreEqual(now.Ticks + 1, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(nowPlusOne.Ticks, _Aircraft.LastCoordinateChanged);
            Assert.AreEqual(nowPlusOne, _Aircraft.LatestCoordinateTime);
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Does_Not_Update_Trail_When_Message_Does_Not_Have_Position()
        {
            var now = DateTime.UtcNow;
            _Aircraft.DataVersion = now.Ticks;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(0, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(0, _Aircraft.FirstCoordinateChanged);
            Assert.AreEqual(0, _Aircraft.LastCoordinateChanged);
            Assert.AreEqual(DateTime.MinValue, _Aircraft.LatestCoordinateTime);
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Copes_When_Messages_Come_In_From_Aircraft_With_No_Track()
        {
            // During initial testing it was found that if an aircraft transmitted three (or more) positions at different
            // locations over at least 3 seconds, but without moving more than 250 metres over those three messages, then
            // a null reference exception was thrown

            var now = DateTime.UtcNow;

            for(int i = 0;i < 3;++i) {
                now = now.AddSeconds(i * 2);

                var latitude = 10f + ((float)i / 100000f);
                _Aircraft.DataVersion = now.Ticks;
                _Aircraft.Latitude = latitude;
                _Aircraft.Longitude = 20;
                _Aircraft.Track = null;

                _Aircraft.UpdateCoordinates(now, 30);

                var lastCoordinate = _Aircraft.FullCoordinates.LastOrDefault();
                Assert.AreEqual(latitude, lastCoordinate.Latitude);
            }
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Ignores_Trail_Updates_Less_Than_A_Second_Apart()
        {
            var time = new DateTime(2001, 1, 1, 19, 01, 10);
            var timePlusPoint999 = time.AddMilliseconds(999);
            var timePlusOne = time.AddSeconds(1);
            var timePlusOnePoint999 = timePlusOne.AddMilliseconds(999);

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = 10;
            _Aircraft.Longitude = 20;
            _Aircraft.UpdateCoordinates(time, 30);

            _Aircraft.DataVersion = timePlusPoint999.Ticks;
            _Aircraft.Latitude = 10.0001f;
            _Aircraft.Longitude = 20.0001f;
            _Aircraft.UpdateCoordinates(timePlusPoint999, 30);

            _Aircraft.DataVersion = timePlusOne.Ticks;
            _Aircraft.Latitude = 10.0002f;
            _Aircraft.Longitude = 20.0002f;
            _Aircraft.UpdateCoordinates(timePlusOne, 30);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);

            Assert.AreEqual(10f, _Aircraft.ShortCoordinates[0].Latitude);
            Assert.AreEqual(10.0002f, _Aircraft.ShortCoordinates[1].Latitude);

            Assert.AreEqual(10f, _Aircraft.FullCoordinates[0].Latitude);
            Assert.AreEqual(10.0002f, _Aircraft.FullCoordinates[1].Latitude);

            // That has the basic stuff covered, but we want to make sure that we're using the latest time and not the time of the first coordinate
            _Aircraft.DataVersion = timePlusOnePoint999.Ticks;
            _Aircraft.Latitude = 10.0003f;
            _Aircraft.Longitude = 20.0003f;
            _Aircraft.UpdateCoordinates(timePlusOnePoint999, 30);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Ignores_Trail_Updates_When_The_Position_Does_Not_Change()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = 51;
            _Aircraft.Longitude = 12;
            _Aircraft.Track = 40;

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Track = 50;
            _Aircraft.UpdateCoordinates(time, 30);

            Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(40f, _Aircraft.FullCoordinates[0].Heading);

            Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(40f, _Aircraft.ShortCoordinates[0].Heading);
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Updates_Trail_When_Altitude_Changes_And_Poisition_Does_Not()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = 51;
            _Aircraft.Longitude = 12;
            _Aircraft.Track = 40;
            _Aircraft.Altitude = 10000;

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Altitude = 11000;
            _Aircraft.UpdateCoordinates(time, 30);

            Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(11000, _Aircraft.FullCoordinates[1].Altitude);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(11000, _Aircraft.ShortCoordinates[1].Altitude);
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Updates_Trail_When_GroundSpeed_Changes_And_Poisition_Does_Not()
        {
            var time = DateTime.UtcNow;

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = 51;
            _Aircraft.Longitude = 12;
            _Aircraft.Track = 40;
            _Aircraft.GroundSpeed = 100;

            _Aircraft.UpdateCoordinates(time, 30);

            time = time.AddSeconds(1);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.GroundSpeed = 150;
            _Aircraft.UpdateCoordinates(time, 30);

            Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);
            Assert.AreEqual(150, _Aircraft.FullCoordinates[1].GroundSpeed);

            Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
            Assert.AreEqual(150, _Aircraft.ShortCoordinates[1].GroundSpeed);
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "FullCoordinateTrim$")]
        public void Aircraft_UpdateCoordinates_Replaces_Last_Full_Coordinate_If_Track_Hardly_Changes()
        {
            // If an update changes the position but the aircraft remains on the same ground track as the previous position
            // then there's no need to keep the previous position, it can be replaced with the current one. 

            var worksheet = new ExcelWorksheetData(TestContext);

            var time = DateTime.Now;

            var expected = new List<Coordinate>();
            for(int i = 1;i <= 3;++i) {
                var latInColumn = String.Format("Lat{0}", i);
                var lngInColumn = String.Format("Lng{0}", i);
                var trkInColumn = String.Format("Trk{0}", i);
                var altInColumn = String.Format("Alt{0}", i);
                var spdInColumn = String.Format("Spd{0}", i);

                var latOutColumn = String.Format("CLat{0}", i);
                var lngOutColumn = String.Format("CLng{0}", i);
                var trkOutColumn = String.Format("CTrk{0}", i);
                var altOutColumn = String.Format("CAlt{0}", i);
                var spdOutColumn = String.Format("CSpd{0}", i);

                if(worksheet.String(latInColumn) != null) {
                    time = time.AddHours(1);
                    _Aircraft.DataVersion = time.Ticks;
                    _Aircraft.Latitude = worksheet.Float(latInColumn);
                    _Aircraft.Longitude = worksheet.Float(lngInColumn);
                    _Aircraft.Altitude = worksheet.NInt(altInColumn);
                    _Aircraft.GroundSpeed = worksheet.NFloat(spdInColumn);
                    _Aircraft.Track = worksheet.NFloat(trkInColumn);
                    _Aircraft.UpdateCoordinates(time, 300 * 60 * 60);

                    Assert.AreEqual(i, _Aircraft.ShortCoordinates.Count);
                }

                if(worksheet.String(latOutColumn) != null) {
                    expected.Add(new Coordinate(0, 0, worksheet.Float(latOutColumn), worksheet.Float(lngOutColumn), worksheet.NFloat(trkOutColumn), worksheet.NInt(altOutColumn), worksheet.NFloat(spdOutColumn)));
                }
            }

            Assert.AreEqual(expected.Count, _Aircraft.FullCoordinates.Count);

            for(int i = 0;i < expected.Count;++i) {
                var expCoordinate = expected[i];
                var fullCoordinate = _Aircraft.FullCoordinates[i];

                Assert.AreEqual(expCoordinate, fullCoordinate, i.ToString());
                Assert.AreEqual(expCoordinate.Heading, fullCoordinate.Heading, i.ToString());
            }
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ShortCoordinatesTrim$")]
        public void Aircraft_UpdateCoordinates_Trims_Short_Coordinates_After_Time_Limit_Has_Passed()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var seconds = worksheet.Int("Seconds");
            var baseTime = DateTime.UtcNow;

            for(int i = 1;i <= 3;++i) {
                var secondsGapColumn = String.Format("SecondsGap{0}", i);
                var latitudeColumn = String.Format("Latitude{0}", i);
                var longitudeColumn = String.Format("Longitude{0}", i);
                var trackColumn = String.Format("Track{0}", i);

                if(worksheet.String(secondsGapColumn) != null) {
                    var utcNow = baseTime.AddSeconds(worksheet.Int(secondsGapColumn));
                    _Aircraft.DataVersion = utcNow.Ticks;
                    _Aircraft.Latitude = worksheet.NFloat(latitudeColumn);
                    _Aircraft.Longitude = worksheet.NFloat(longitudeColumn);
                    _Aircraft.Track = worksheet.NFloat(trackColumn);
                    _Aircraft.UpdateCoordinates(utcNow, seconds);
                }
            }

            Assert.AreEqual(worksheet.Int("ExpectedCount"), _Aircraft.ShortCoordinates.Count);
            for(int i = 0;i < _Aircraft.ShortCoordinates.Count;++i) {
                var expectedTrackColumn = String.Format("ExpectedTrack{0}", i);
                Assert.AreEqual(worksheet.NFloat(expectedTrackColumn), _Aircraft.ShortCoordinates[i].Heading, i.ToString());
            }
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ResetCoordinates$")]
        public void Aircraft_UpdateCoordinates_Resets_Coordinates_If_Aircraft_Looks_To_Be_Moving_Impossibly_Quickly()
        {
            // Some aircraft's transponders report the wrong position. Further, some radios misinterpret position updates at
            // extreme ranges. We can guess the position is wrong if the position and timing of two updates would require the
            // aircraft to be moving at incredibly high speeds. When this is detected we should reset the trails, otherwise the
            // user sees long lines drawn across the map, or a scribble effect if the transponder is just reporting nonsense.

            var worksheet = new ExcelWorksheetData(TestContext);

            var time = DateTime.UtcNow;
            var shortSeconds = 24 * 60 * 60; // seconds in a day

            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = worksheet.Float("Lat1");
            _Aircraft.Longitude = worksheet.Float("Lng1");
            _Aircraft.Track = 10;
            _Aircraft.UpdateCoordinates(time, shortSeconds);

            int milliseconds = worksheet.Int("Milliseconds");

            time = time.AddMilliseconds(milliseconds);
            _Aircraft.DataVersion = time.Ticks;
            _Aircraft.Latitude = worksheet.Float("Lat2");
            _Aircraft.Longitude = worksheet.Float("Lng2");
            _Aircraft.Track = 20;
            _Aircraft.UpdateCoordinates(time, shortSeconds);

            if(!worksheet.Bool("ResetTrail")) {
                if(milliseconds >= 1000) {
                    Assert.AreEqual(2, _Aircraft.FullCoordinates.Count);
                    Assert.AreEqual(2, _Aircraft.ShortCoordinates.Count);
                } else {
                    Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
                    Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);
                    Assert.AreEqual(10, _Aircraft.FullCoordinates[0].Heading);
                    Assert.AreEqual(10, _Aircraft.ShortCoordinates[0].Heading);
                }
            } else {
                if(milliseconds < 1000) {
                    Assert.AreEqual(0, _Aircraft.FullCoordinates.Count);
                    Assert.AreEqual(0, _Aircraft.ShortCoordinates.Count);
                } else {
                    Assert.AreEqual(1, _Aircraft.FullCoordinates.Count);
                    Assert.AreEqual(1, _Aircraft.ShortCoordinates.Count);
                    Assert.AreEqual(20, _Aircraft.FullCoordinates[0].Heading);
                    Assert.AreEqual(20, _Aircraft.ShortCoordinates[0].Heading);
                }
            }
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Sets_PositonTime_When_Latitude_And_Longitude_Are_Changed()
        {
            DateTime now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks;
            _Aircraft.Latitude = 1;
            _Aircraft.Longitude = 1;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime);
        }

        [TestMethod]
        public void Aircraft_UpdateCoordinates_Does_Not_Set_PositionTime_If_Coordinates_Have_Not_Changed_Since_Last_Update()
        {
            DateTime now = DateTime.UtcNow;

            _Aircraft.DataVersion = now.Ticks;
            _Aircraft.Latitude = 1;
            _Aircraft.Longitude = 1;
            _Aircraft.UpdateCoordinates(now, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime);

            var newTime = now.AddSeconds(10);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(now, _Aircraft.PositionTime);

            newTime = now.AddSeconds(20);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.Latitude = 2;
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(newTime, _Aircraft.PositionTime);

            newTime = now.AddSeconds(25);
            _Aircraft.DataVersion = newTime.Ticks;
            _Aircraft.Longitude = 2;
            _Aircraft.UpdateCoordinates(newTime, 30);

            Assert.AreEqual(newTime, _Aircraft.PositionTime);
        }
        #endregion
    }
}
