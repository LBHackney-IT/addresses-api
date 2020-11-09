


# Addresses
## Changes to the address response object
These changes will be relevent to both address endpoints
1. The `Approved Preferred` option of the `addressStatus` property is changing to `Approved`. Making the new possible values:
```[ Approved, Historial, Alternative, Provisional ]```.
2. The `hackneyGazetteerOutOfBoroughAddress` property has been replaced with the `outOfBoroughAddress` property, and denotes whether or not, an address is within the hackney borough.

## Changes to the get single address endpoint
`/api/v2/addresses/{addressKey}`

1. If an address can not be found for the address key provided, the API will now return a 404 status code instead of a 200 status code.
2. If the address key is invalid, it will return a 400 status code.

3. Currently a successful response will look like:
```json
{
   "statusCode": 200,
   "data":  {
     "addressess": [
       {
         "addressKey": "HDYAGFYADIAGFJSJ",
         "uprn": 98492847812
         ...
       }
     ]
   }
}
```
going forward it will look like:
```json
{
   "statusCode": 200,
   "data":  {
     "address": {
        "addressKey": "HDYAGFYADIAGFJSJ",
         "uprn": 98492847812
        ...
      }
   }
}
```

## Changes to the search addresses endpoint
`/api/v2/addresses`

1. All request parameters are now snake case instead of camel case, namely:
  -  `BuildingNumber` -> `building_number`
  -  `usagePrimary` -> `usage_primary`
  -  `usageCode` -> `usage_code`
  -  `AddressStatus` -> `address_status`
  -  `PageSize` -> `page_size`
2. Page count and total count properties in the response are now camel case instead of snake case. i.e. `page_count` is not `pageCount` and `total_count` is now `totalCount`.
3. The response in event of a error has changed slightly. Currently error responses look like:
```json
{
  "statusCode": 400,
  "error": {
    "isValid": false,
    "validationErrors": [
      {
        "fieldName": "Format",
        "message": "Value for Format is not valid. It should be either Simple or Detailed"
      }
    ]
  }
}
```
Going forward they will look like:
```json
{
  "statusCode": 400,
  "errors": [
    {
      "fieldName": "Format",
      "message": "Value for Format is not valid. It should be either Simple or Detailed"
    }
  ]
}
```

# Properties
## Get cross references for a propery
`​/api​/v2​/properties​/{uprn}​/crossreferences`

There are no changes to this endpoint.




