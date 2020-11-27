using System.Collections.Generic;
using AddressesAPI.V2;
using AddressesAPI.V2.Boundary.Requests;
using AddressesAPI.V2.UseCase;
using FluentAssertions;
using FluentValidation.TestHelper;
using NUnit.Framework;

namespace AddressesAPI.Tests.V2.UseCase
{
    [TestFixture]
    public class SearchAddressValidatorTests
    {
        private SearchAddressValidator _classUnderTest;
        private string _localGazetteer;
        private string _nationalGazetteer;

        [SetUp]
        public void SetUp()
        {
            _localGazetteer = GlobalConstants.AddressScope.HackneyGazetteer.ToString();
            _nationalGazetteer = GlobalConstants.AddressScope.National.ToString();
            _classUnderTest = new SearchAddressValidator();
        }

        #region Address status validation
        [TestCase("cat")]
        [TestCase("provizional")]
        [TestCase("alternative,hystorical")]
        public void GivenAnAddressStatusValueThatDoesntMatchAllowedOnes_WhenCallingValidation_ItReturnsAnError(string addressStatusVal)
        {
            var request = new SearchAddressRequest { AddressStatus = addressStatusVal };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.AddressStatus, request)
                .WithErrorMessage("Value for the parameter is not valid.");
        }

        [TestCase("alternative")]
        [TestCase("historical")]
        [TestCase("approved,historical")]
        public void GivenAnAllowedAddressStatusValue_WhenCallingValidation_ItReturnsNoErrors(string addressStatusVal)
        {
            var request = new SearchAddressRequest { AddressStatus = addressStatusVal };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.AddressStatus, request);
        }

        [TestCase(" ")]
        [TestCase("")]
        [TestCase("alternative,  ,something")]
        [TestCase(null)]
        public void GivenAWhitespaceOrEmptyAddressStatusValue_WhenCallingValidation_ItReturnsAnError(string addressStatusVal)
        {
            var request = new SearchAddressRequest() { AddressStatus = addressStatusVal };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.AddressStatus, request);
        }
        #endregion
        // Below explanations all use the postcodes IG11 7QD and E5 3XW
        //"Incode" refers to the whole second part of the postcode (i.e. 3XW, 7QD) from (E11 3XW, W3 7QD)
        //"Outcode" refers to the whole first part of the postcode (Letter(s) and number(s) - i.e. IG11, E5) from (IG11 9LL, E5 2LL)
        //"Area" refers to the first letter(s) of the postcode (i.e.  IG, E) from (IG11 9LL, E5 2LL)
        //"District" refers to first number(s) to appear in the postcode (i.e. 11, 5) from (IG11 9LL, E5 2LL)
        //"Sector" refers to the number in the second part of the postcode (i.e. 9, 7) from (SW2 9DN, NE4 7JU)
        //"Unit" refers to the letters in the second part of the postcode (i.e. DN, JU) from (SW2 9DN, NE4 7JU)
        #region Postcode validation
        [TestCase("CR1 3ED")]
        [TestCase("NE7")]
        public void GivenAPostCodeValueInUpperCase_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("w2 5jq")]
        [TestCase("ne7")]
        public void GivenAPostCodeValueInLowerCase_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("w2 5JQ")]
        [TestCase("E11 5ra")]
        public void GivenAPostCodeValueInLowerCaseAndUpperCase_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("w2 ")]
        [TestCase("E11 ")]
        public void GivenAnOutcodeWithSpace_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("CR13ED")]
        [TestCase("RE15AD")]
        public void GivenPostCodeValueWithoutSpaces_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("NW")]
        [TestCase("E")]
        public void GivenOnlyAnAreaPartOfThePostCode_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        [TestCase("17 9LL")]
        [TestCase("8 1LA")]
        public void GivenOnlyAnIncodeAndADistrictPartsOfThePostCode_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        [TestCase("NW 9LL")]
        [TestCase("NR1LW")]
        public void GivenOnlyAnIncodeAndAnAreaPartsOfThePostCode_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        [TestCase("1LL")]
        [TestCase(" 6BQ")]
        public void GivenOnlyAnIncodePartOfThePostCode_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        [TestCase("E9")] //A9
        [TestCase("S5")]
        [TestCase("S11")] //A99
        [TestCase("W12")]
        [TestCase("NW9")] //AA9
        [TestCase("RH5")]
        [TestCase("SW17")] // AA99
        [TestCase("NE17")]
        [TestCase("W4R")] // A9A
        [TestCase("N1C")]
        [TestCase("NW1W")] // AA9A
        [TestCase("CR1H")]
        public void GivenOnlyAnOutcodePartOfPostCode_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("E8 1LL")]
        [TestCase("SW17 1JK")]
        public void GivenBothPartsOfPostCode_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("IG117QDfdsfdsfd")]
        [TestCase("E1llolol")]
        public void GivenAValidPostcodeFolowedByRandomCharacters_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        [TestCase("EEE")]
        [TestCase("THE")]
        public void GivenThreeCharacters_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        [TestCase("SW11 9")]
        [TestCase("e14 2")]
        public void GivenAnOutcodeAndASector_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("SW7 9A")]
        [TestCase("n12 8F")]
        public void GivenAnOutcodeAndASectorAndTheFirstLetterOfTheUnit_WhenCallingValidation_ItReturnsNoErrors(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Postcode, request);
        }

        [TestCase("N8 LL")]
        [TestCase("NW11 AE")]
        public void GivenAnOutcodeAndAUnit_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        [TestCase("S10 H")]
        [TestCase("W1 J")]
        public void GivenAnOutcodeAndOnlyOneLetterOfAUnit_WhenCallingValidation_ItReturnsAnError(string postCode)
        {
            var request = new SearchAddressRequest() { Postcode = postCode };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Postcode, request).WithErrorMessage("Must provide at least the first part of the postcode.");
        }

        #endregion
        #region QueryParameterValidation

        [TestCase("asdfghjk", "postcode", "RM3 0FS")]
        [TestCase("ccvbbvv", "postcode", "E5 0DW")]
        public void GivenAnInvalidFilterParameterAndMandatoryParameter_WhenCallingValidation_ItReturnsAnError(string queryParameter1, string queryParameter2, string postcode)
        {
            var queryStringParameters = new List<string>() { queryParameter1, queryParameter2 };
            var request = new SearchAddressRequest() { Postcode = postcode, RequestFields = queryStringParameters };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.RequestFields, request).WithErrorMessage("Invalid properties have been provided.");
        }

        [TestCase("uprn", "postcode", "RM3 0FS")]
        [TestCase("usrn", "postcode", "E5 0DW")]
        public void GivenOnlyValidFilterParameters_WhenCallingValidation_ItReturnsNoErrors(string queryParameter1, string queryParameter2, string postcode)
        {
            var queryStringParameters = new List<string>() { queryParameter1, queryParameter2 };
            var request = new SearchAddressRequest() { Postcode = postcode, RequestFields = queryStringParameters };

            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.RequestFields, request);
        }

        [TestCase("RequestFields", "postcode", "E5 9TT")]
        [TestCase("Errors", "postcode", "E7 2JT")]
        public void GivenInvalidFilterParameterWhoseNameMatchesOneOfSearchAddressRequestPropertiesThatAreNotUsedToGetOrFilterData_WhenCallingValidation_ItReturnsAnError(string queryParameter1, string queryParameter2, string postcode) //we also provide postcode, because it's mandatory and the other validation will interfere with this test if it's not put in.
        {
            var queryStringParameters = new List<string>() { queryParameter1, queryParameter2 };
            var request = new SearchAddressRequest() { Postcode = postcode, RequestFields = queryStringParameters };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.RequestFields, request).WithErrorMessage("Invalid properties have been provided.");
        }

        [TestCase("ubrn", "addrezzstatus", "postcode", "RM3 0FS")]
        [TestCase("uzzrn", "gazziityr", "postcode", "E5 0DW")]
        public void GivenMultipleInvalidFilterParametersAndMandatoryParameter_WhenCallingValidation_ItReturnsAnError(string queryParameter1, string queryParameter2, string queryParameter3, string postcode)
        {
            var queryStringParameters = new List<string>() { queryParameter1, queryParameter2, queryParameter3 };
            var request = new SearchAddressRequest() { Postcode = postcode, RequestFields = queryStringParameters };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.RequestFields, request).WithErrorMessage("Invalid properties have been provided.");
        }

        [TestCase("cross_ref_code", "cross_ref_value", "postcode", "RM3 0FS")]
        [TestCase("include_parent_shells", "address_status", "postcode", "E3 1QW")]
        public void GivenValidFilterParametersContainingUnderscores_WhenCallingValidation_ItReturnsNoErrors(string queryParameter1, string queryParameter2, string queryParameter3, string postcode)
        {
            var queryStringParameters = new List<string>() { queryParameter1, queryParameter2, queryParameter3 };
            var request = new SearchAddressRequest() { Postcode = postcode, RequestFields = queryStringParameters };

            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.RequestFields, request);
        }

        #endregion
        #region Request object validation

        [TestCase(12345)]
        public void GivenARequestWithOnlyUPRN_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsNoError(int uprn)
        {
            var request = new SearchAddressRequest() { UPRN = uprn, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase(12345)]
        public void GivenARequestWithOnlyUPRN_IfAddressScopeIsHackneyNational_WhenCallingValidation_ItReturnsNoError(int uprn)
        {
            var request = new SearchAddressRequest() { UPRN = uprn, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase(12345)]
        public void GivenARequestWithOnlyUSRN_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsNoError(int usrn)
        {
            var request = new SearchAddressRequest() { USRN = usrn, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase(12345)]
        public void GivenARequestWithOnlyUSRN_IfAddressScopeIsNational_WhenCallingValidation_ItReturnsNoError(int usrn)
        {
            var request = new SearchAddressRequest() { USRN = usrn, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("SW1A 1AA")]
        public void GivenARequestWithOnlyAPostCode_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsNoError(string postcode)
        {
            var request = new SearchAddressRequest() { Postcode = postcode, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("SW1A 1AA")]
        public void GivenARequestWithOnlyAPostCode_IfAddressScopeIsNational_WhenCallingValidation_ItReturnsNoError(string postcode)
        {
            var request = new SearchAddressRequest() { Postcode = postcode, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("hackney road")]
        public void GivenARequestWithOnlyAnAddressQuery_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsNoError(string query)
        {
            var request = new SearchAddressRequest() { Query = query, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("liverpool road")]
        public void GivenARequestWithOnlyAnAddressQuery_IfAddressScopeIsNational_WhenCallingValidation_ItReturnsNoError(string query)
        {
            var request = new SearchAddressRequest() { Query = query, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("Sesame street")]
        public void GivenARequestWithOnlyAStreet_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsNoError(string street)
        {
            var request = new SearchAddressRequest() { Street = street, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("Sesame street")]
        public void GivenARequestWithOnlyAStreet_IfAddressScopeIsNational_WhenCallingValidation_ItReturnsAnError(string street)
        {
            var request = new SearchAddressRequest() { Street = street, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldHaveError().WithErrorMessage(
                "You must provide at least one of (query, uprn, usrn, postcode), when address_scope is 'national'.");
        }

        [TestCase("someValue")]
        public void GivenARequestWithOnlyUsagePrimary_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsNoError(string usagePrimary)
        {
            var request = new SearchAddressRequest() { UsagePrimary = usagePrimary, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("someValue")]
        public void GivenARequestWithOnlyUsagePrimary_IfAddressScopeIsNational_WhenCallingValidation_ItReturnsAnError(string usagePrimary)
        {
            var request = new SearchAddressRequest() { UsagePrimary = usagePrimary, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldHaveError().WithErrorMessage(
                "You must provide at least one of (query, uprn, usrn, postcode), when address_scope is 'national'.");
        }

        [TestCase("otherValue")]
        public void GivenARequestWithOnlyUsageCode_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsNoError(string usageCode)
        {
            var request = new SearchAddressRequest() { UsageCode = usageCode, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [TestCase("otherValue")]
        public void GivenARequestWithOnlyUsageCode_IfAddressScopeIsNational_WhenCallingValidation_ItReturnsAnError(string usageCode)
        {
            var request = new SearchAddressRequest() { UsageCode = usageCode, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldHaveError().WithErrorMessage(
                "You must provide at least one of (query, uprn, usrn, postcode), when address_scope is 'national'.");
        }

        [TestCase("12345")]
        public void GivenARequestWithNoMandatoryFields_IfAddressScopeIsHackneyGazetteer_WhenCallingValidation_ItReturnsAnError(string buildingNumber)
        {
            var request = new SearchAddressRequest() { BuildingNumber = buildingNumber, AddressScope = _localGazetteer };
            _classUnderTest.TestValidate(request).ShouldHaveError().WithErrorMessage(
                "You must provide at least one of (query, uprn, usrn, postcode, street, usagePrimary, usageCode), when address_scope is 'hackney borough' or 'hackney gazetteer'.");
        }

        [TestCase("12345")]
        public void GivenARequestWithNoMandatoryFields_IfAddressScopeIsNational_WhenCallingValidation_ItReturnsAnError(string buildingNumber)
        {
            var request = new SearchAddressRequest() { BuildingNumber = buildingNumber, AddressScope = _nationalGazetteer };
            _classUnderTest.TestValidate(request).ShouldHaveError().WithErrorMessage(
                "You must provide at least one of (query, uprn, usrn, postcode), when address_scope is 'national'.");
        }

        [Test]
        public void GivenARequestWithCrossReference_WithoutAPostcodeOrUPRN_WhenCallingValidation_ItReturnsNoErrors()
        {
            const string code = "TESTCT";
            const string referenceValue = "900000";

            var request = new SearchAddressRequest() { CrossRefCode = code, CrossRefValue = referenceValue };
            _classUnderTest.TestValidate(request).ShouldNotHaveError();
        }

        [Test]
        public void GivenARequestForCrossReferenceWithEitherParameterForCodeOrValueMissing_WhenCallingValidation_ItReturnsAnErrorRelatingToCrossReference()
        {
            const string code = "HELLO";
            const string referenceValue = "1234GHIJK";

            var requestOne = new SearchAddressRequest() { CrossRefCode = code };
            _classUnderTest.TestValidate(requestOne).ShouldHaveError().WithErrorMessage("You must provide both the code and a value, when searching by a cross reference");

            var requestTwo = new SearchAddressRequest() { CrossRefValue = referenceValue };
            _classUnderTest.TestValidate(requestTwo).ShouldHaveError().WithErrorMessage("You must provide both the code and a value, when searching by a cross reference");
        }

        [TestCase("123XYZ", "450000")]
        [TestCase(null, null)]
        public void GivenARequestWithCrossRefCodeAndACrossRefValue_BothPresentOrBothAbsent_WhenCallingValidation_ItReturnsNoErrorRelatingToCrossReference(string crossRefCode, string value)
        {
            var request = new SearchAddressRequest() { CrossRefCode = crossRefCode, CrossRefValue = value };

            _classUnderTest.TestValidate(request).Result.Errors.Should().NotContain("You must provide both the code and a value, when searching by a cross reference");
        }

        #endregion

        #region Enum validation

        [TestCase("long")]
        [TestCase("verbose")]
        [TestCase("short")]
        public void GivenAnIncorrectFormat_WhenValidating_ItReturnsAnError(string format)
        {
            var request = new SearchAddressRequest { Format = format };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Format, request)
                .WithErrorMessage("Value for Format is not valid. It should be either Simple or Detailed");
        }

        [TestCase("Simple")]
        [TestCase("simple")]
        [TestCase("Detailed")]
        public void GivenACorrectFormat_WhenValidating_ItReturnsNoErrors(string format)
        {
            var request = new SearchAddressRequest { Format = format };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.Format, request);
        }

        [TestCase("finsburypark")]
        [TestCase("haringay")]
        [TestCase("dalston")]
        [TestCase("Both")]
        [TestCase("Hackney")]
        public void GivenAnIncorrectAddressScope_WhenValidating_ItReturnsAnError(string addressScope)
        {
            var request = new SearchAddressRequest { AddressScope = addressScope };
            _classUnderTest.ShouldHaveValidationErrorFor(x => x.AddressScope, request)
                .WithErrorMessage("Value for the parameter is not valid. It should be either Hackney Borough, Hackney Gazetteer or National.");
        }

        [TestCase("hackney borough")]
        [TestCase("hackneyborough")]
        [TestCase("Hackney Borough")]
        [TestCase("hackney gazetteer")]
        [TestCase("hackneygazetteer")]
        [TestCase("Hackney Gazetteer")]
        [TestCase("national")]
        [TestCase("National")]
        public void GivenACorrectAddressScope_WhenValidating_ItReturnsNoErrors(string addressScope)
        {
            var request = new SearchAddressRequest { AddressScope = addressScope };
            _classUnderTest.ShouldNotHaveValidationErrorFor(x => x.AddressScope, request);
        }

        #endregion
    }
}
