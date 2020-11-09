


# Addresses
## Changes to the address response object
1. The `Approved Preferred` option of the `addressStatus` property is changing to `Approved`. Making the new possible values:
```[ Approved, Historial, Alternative, Provisional ]```.

## Changes to the get single address endpoint
`/api/v2/addresses/{addressKey}`

1. If an address can not be found for the address key provided, the API will now return a 404 status code instead of a 200 status code.
2. It will return a 400 if the address key is invalid.

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
4.



## Search Addresses
`/api/v2/addresses`

# Properties
## Get cross references for a propery
`​/api​/v2​/properties​/{uprn}​/crossreferences`

# Across all endpoints
### Error response structure
Currently error responses look like:
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




