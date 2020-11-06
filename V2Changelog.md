
# Get Address

If an address can not be found for the address key provided, the API will now return a 404 status code instead of a 200 status code.

Returns a 404 if address cant be found
Returns a 400 if address key is invalid

# Search Addresses
## Changes to Address Response Object

Currently the address response object looks like:
```json
{

}
```
1. The `Approved Preferred` option of the `addressStatus` property is changing to `Approved`. Making the new possible values:
```[ Approved, Historial, Alternative, Provisional ]```.

## Error response structure
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
