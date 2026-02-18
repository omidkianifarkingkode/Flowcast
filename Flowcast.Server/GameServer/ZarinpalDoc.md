####Post request####
curl -X POST \
  https://payment.zarinpal.com/pg/v4/payment/request.json \
  -H 'accept: application/json' \
  -H 'content-type: application/json' \
  -d '{
  "merchant_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "amount": 1000,
  "callback_url": "http://your-site.com/verify",
  "referrer_id": "xxxx",
  "description": "Transaction description.",
  "metadata": {"mobile": "09121234567","email": "info.test@gmail.com"}
}

####Response####
{
  "data": {
    "code": 100,
    "message": "Success",
    "authority": "A0000000000000000000000000000wwOGYpd",
    "fee_type": "Merchant",
    "fee": 100
  },
  "errors": []
}

####Take player to purchase page ####
Location: https://payment.zarinpal.com/pg/StartPay/ . $result['data']["authority"]

Return to the app page :
http://www.yoursite.ir/?Authority=A0000000000000000000000000000wwOGYpd&Status=OK

####Validationg####
Post Mthod
https://payment.zarinpal.com/pg/v4/payment/verify.json

curl -X POST \
  https://payment.zarinpal.com/pg/v4/payment/verify.json \
          -H 'accept: application/json' \
  -H 'content-type: application/json' \
  -d '{
"merchant_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
        "amount": 1000,
        "authority": "A0000000000000000000000000000wwOGYpd"
}'

####Validate Response ####
{
  "data": {
    "code": 100,
    "message": "Verified",
    "card_hash": "1EBE3EBEBE35C7EC0F8D6EE4F2F859107A87822CA179BC9528767EA7B5489B69",
    "card_pan": "502229******5995",
    "ref_id": 201,
    "fee_type": "Merchant",
    "fee": 0
  },
  "errors": []
}

####Error List####
https://www.zarinpal.com/docs/paymentGateway/errorList.html



#### Automatic / Manually Validating #####
curl -X POST \
  https://payment.zarinpal.com/pg/v4/payment/request.json \
  -H 'accept: application/json' \
  -H 'content-type: application/json' \
  -d '{
  "merchant_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "currency": "IRT",
  "amount": 10000,
  "callback_url": "https://example.com/payment-callback",
  "description": "خرید اشتراک",
  "metadata": {
    "auto_verify": true,
    "mobile": "09121234567",
    "email": "info@example.com"
  }
}'

#### Checkout Request Example ####
curl -X POST \
  https://payment.zarinpal.com/pg/v4/payment/request.json \
  -H 'accept: application/json' \
  -H 'content-type: application/json' \
  -d '{
  "merchant_id": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "amount": 150000,
  "description": "پرداخت مربوط به سفارش شماره 1010 در فروشگاه تستی",
  "metadata": {
    "mobile": "09120000000",
    "email": "user@example.com",
    "order_id": "1234567890",
    "card_pan": "502229XXXXXX1234"
  },
  "cart_data": {
    "items": [
      {
        "item_name": "کفش ورزشی نایک",
        "item_amount": 50000,
        "item_count": 2,
        "item_amount_sum": 100000
      },
      {
        "item_name": "جوراب اسپرت",
        "item_amount": 25000,
        "item_count": 1,
        "item_amount_sum": 25000
      }
    ],
    "added_costs": {
      "tax": 5000,
      "payment": 1000,
      "transport": 2000
    },
    "deductions": {
      "discount": 3000
    }
  },
  "callback_url": "https://example.com/payment/verify"
}'
