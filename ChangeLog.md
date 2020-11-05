Error responses will now look like:
```json
{
  statusCode: 400,
  errors: [
    {
      "fieldName": "Format",
      "message": "Value for Format is not valid. It should be either Simple or Detailed"
    }
  ]
}
```
previously they looked like:
```json
{
  statusCode: 400,
  error: {
    isValid: false,
    validationErrors: [
      {
        "fieldName": "Format",
        "message": "Value for Format is not valid. It should be either Simple or Detailed"
      }
    ]
  }
}
```
