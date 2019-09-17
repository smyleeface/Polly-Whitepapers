# Polly Whitepapers (.Net Core)

- Install .Net Core 2.2.300
- `dotnet tool install -g LambdaSharp.Tool --version 0.6.0.3`
- `lash config --cli-profile Default`
- `lash init --cli-profile Default --tier Development`
- `cd PollyWhitepapers/dotnet/src`
- `lash deploy --cli-profile Default --tier Development`
- When prompted, enter your email address
- After deployment is complete, check your email and approve the email subscription

- Open PollyWhitepapers/dotnet/src/Module.yml; uncomment lines 14-43
- `lash deploy --cli-profile Default --tier Development`

- Find the S3 bucket that was created (the S3 bucket name should have the word `polly` and `whitepapers` in it)
- Create a folder called `raw`
- Upload a PDF to the `raw` folder

- After some time, you should receive an email notification that the polly job is complete