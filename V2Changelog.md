# Addresses
## Changes to the address response object
These changes will be relevent to both address endpoints
1. The `Approved Preferred` option of the `addressStatus` property is changing to `Approved`. Making the new possible values:
```[ Approved, Historial, Alternative, Provisional ]```.
2. The `hackneyGazetteerOutOfBoroughAddress` property has been replaced with the `outOfBoroughAddress` property, and denotes whether or not, an address is within the borough of hackney .

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
4. The `Approved Preferred` option of the `address_status` query parameter is changing to `Approved`. Making the new possible values:
   ```[ Approved, Historial, Alternative, Provisional ]```.
5. There is a new query parameter `address_scope` allowing you to choose which set of addresses to search for your address in. It has the following options ```[ Hackney Borough, Hackney Gazetteer, National ]```. `Hackney Borough` will search addresses within hackney, `Hackney Gazetteer` will search addresses managed by hackney council and `National` will search all addresses in both the national and hackney gazetteer. Default is `Hackney Borough`.
6. The `gazetteer` query parameter has been removed. The `address_scope` query parameter mentioned in 5 provides this functionality.
7. There is a new query parameter `include_property_shells` which wll toggle whether or not to include the parent shells of addresses which are returned. If it is set to true, the endpoint will return all addresses which would be returned normally plus any parent shells and parents of parent shells and so on.
An example of a parent shell would be a house converted into flats, each flat in the house would have it's own address (e.g. 1st Floor Flat, 5 Hackney Avenue) and then it's parent shell would be the address of the house (e.g. 5 Hackney Avenue).
8. Two new optional query parameters have been added to allow a user to filter addresses by a specific cross reference. These are `cross_ref_code` and `cross_ref_value`. If filtering by a cross reference, both must be supplied i.e You cannot supply just a `cross_ref_code`. If only one has been supplied, the endpoint will return 400 (Bad Request) error. When both are given, the endpoint will return all addresses that match the cross reference given.
9. There is a new query parameter `query` which allows you to search the address text. It will search against all lines of the address text, enabling you to partially search addresses and allowing some misspellings and common mistakes. It will return more relevent matches first in the list of results.
10. There is a new query parameter `modified_since` which will only return addresses which have been modified since the specified date. The date should be in the format YYYY-MM-DD.

## Changes to the search addresses request validation
1. When filtering by cross reference (i.e. when a specific `cross_ref_code` & `cross_ref_value` is given in the request), there is no obligation for the user to also pass in a postcode, UPRN, USRN, street or usageCode as part of the request. The `address_scope` in the request is also forced to be set to `Hackney Borough` (i.e does not include out of borough addresses). This is because when querying by cross reference, we only want to filter through addresses in the Hackney Borough.
2. The page and page_size query parameters are being validated to make sure they are not negative values.

# Properties
## Get cross references for a property
`​/api​/v2​/properties​/{uprn}​/crossreferences`

There are no changes to this endpoint.
