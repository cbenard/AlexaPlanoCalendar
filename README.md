# About
AlexaPlanoCalendar was created for [me](https://twitter.com/cbenard) to experiment with building Alexa Skills. It fetches data from the [City of Plano](http://plano.gov)'s public [calendar](http://plano.gov/rss.aspx#calendar) via RSS.

# Setting up a new function like this one
I used [this guide](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-coreclr-deployment-package.html) from Amazon.

1. To install the templates for creating new Lambda functions, run: `dotnet new -i Amazon.Lambda.Templates`.
2. Create a new function by running (Region is an AWS region like `us-east-1` and profile can be left as `default`. Change the name to the name of your project.): `dotnet new lambda.EmptyFunction --name MyFunction --profile default --region region`
3. Rearrange the folders and create a sln, if desired, with `dotnet new sln --name YourSolutionName` and add the projects with `dotnet sln YourSolutionName.sln add MyFunction/*.csproj MyFunctionTests/*.csproj`
4. If you want to use fewer resources (and possibly have lower billing), change `"function-memory-size"` in `aws-lambda-tools-defaults.json` to a lower value than the default, like `128`;
5. Install the Alexa.NET NuGet package to have the right Alexa Skill-specific objects for your function: `dotnet add src/MyFunction package Alexa.NET`

# Deploying the function
Again, I used [this guide](https://docs.aws.amazon.com/lambda/latest/dg/lambda-dotnet-coreclr-deployment-package.html) from Amazon.

1. Install the Lambda command by running: `dotnet tool install -g Amazon.Lambda.Tools`
2. Deploy the function with (role comes from the AWS [IAM Roles](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles.html)): `dotnet lambda deploy-function MyFunction –-function-role role`
3. [Create your Alexa Skill](https://developer.amazon.com/docs/custom-skills/steps-to-build-a-custom-skill.html) and link the new function from AWS Lambda to your skill. I have included the my Interaction Model JSON in this GIT Repository.

# License
This project is licensed with the MIT License. A copy of the license should be redistributed with any copy of the code or derivatives.

# Pull requests welcome
If you have suggestions on how to improve my code or add new features, please clone the repository, create a feature branch, and send a pull request.

# Acknowledgements
Nate Barbettini (@nbarbettini) has a great repo called [AlexaHelloWorld.NET](https://github.com/nbarbettini/AlexaHelloWorld.NET) which helped jumpstart my understanding of dotnet core Lambda functions and their interactions with Alexa.