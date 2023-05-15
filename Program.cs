// // See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Tesseract; // if no need change it also can be try ironocr
using System.Diagnostics;
using SkiaSharp;

// Console.WriteLine("Hello, World!");


// IWebDriver driver = new ChromeDriver();
// driver.Navigate().GoToUrl("https://ebelge.gib.gov.tr/efaturakayitlikullanicilar.html");

// // Find the <img> element

// WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
// IWebElement imgElement = wait.Until(driver =>
// {
//     try
//     {
//         var elem = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/img"));
//         if (elem.Displayed)
//         {
//             return elem;
//         }
//         return null;
//     }
//     catch (NoSuchElementException)
//     {
//         return null;
//     }
// });
// //  = driver.FindElement(By.XPath("/html/body/form/table/tbody/tr[2]/td[2]/img"));
// Console.WriteLine("deneme");
// // Get the source URL of the image
// string imageUrl = imgElement.GetAttribute("src");

// // Download the image
// using (HttpClient client = new HttpClient())
// {
//     Console.WriteLine("is http client exist");
//     byte[] imageData = client.GetByteArrayAsync(imageUrl).Result;

//     // Save the image to a file
//     string fileName = "image.jpg";  // Replace with your desired file name
//     System.IO.File.WriteAllBytes(fileName, imageData);

//     System.Console.WriteLine("Image downloaded and saved successfully.");
// }

// driver.Quit();

string ConvertTesseractOcr( string imgPath ) {
    Console.WriteLine("image path is in " + imgPath);

    using (var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default)) 
    {
        using (var img = Pix.LoadFromFile(imgPath))
        {

            using ( var page = engine.Process(img) )
            {
                string text = page.GetText();

                Console.WriteLine("Ocr Output: ");
                Console.WriteLine(text);

                return text;
            }
        }

    }
    throw new Exception("An Error Code.");
}

string ConvertPythonOcr( string imgPath )
{

    Process process = new Process();
    ProcessStartInfo startInfo = new ProcessStartInfo();
    startInfo.FileName = @"python3"; // the name of the script to run :TODO: unbundle full path
    startInfo.Arguments = @"convertOcrProcess.py";
    startInfo.UseShellExecute = false; // don't use the shell to execute the command
    startInfo.RedirectStandardOutput = true; // redirect the output to a stream
    process.StartInfo = startInfo;
    process.Start();

    // read the output of the command
    string output = process.StandardOutput.ReadToEnd();

    // wait for the command to finish
    process.WaitForExit();

    return output;
} 



IWebDriver driver = new ChromeDriver();
driver.Navigate().GoToUrl("https://ebelge.gib.gov.tr/efaturakayitlikullanicilar.html");

IWebElement iframeElement = driver.FindElement(By.Id("blockrandom"));
driver.SwitchTo().Frame(iframeElement);

IWebElement innerIframeElement = driver.FindElement(By.CssSelector("body > div > div > iframe"));
driver.SwitchTo().Frame(innerIframeElement);



//Notice navigation is slightly different than the Java version
//This is because 'get' is a keyword in C#
IWebElement vkOrTcNo = driver.FindElement(By.Name("search_string"));
IWebElement securityCode = driver.FindElement(By.Name("captcha_code"));

IWebElement securityCodeImageElem = driver.FindElement(By.CssSelector("body > form > table > tbody > tr:nth-child(2) > td:nth-child(2) > img"));

int x = securityCodeImageElem.Location.X;
int y = securityCodeImageElem.Location.Y;
int width = securityCodeImageElem.Size.Width;
int height = securityCodeImageElem.Size.Height;

Screenshot screenshot = ((ITakesScreenshot)driver).GetScreenshot();

 // Convert the screenshot to a byte array
byte[] screenshotBytes = screenshot.AsByteArray;

// Create a SKBitmap from the screenshot byte array
using (MemoryStream stream = new MemoryStream(screenshotBytes))
using (SKBitmap bitmap = SKBitmap.Decode(stream))
{
    // Define the region to crop
    SKRectI cropRect = new SKRectI(x, y, width, height); // Replace with your desired values

    // Create a new SKBitmap for the cropped image
    using (SKBitmap croppedBitmap = new SKBitmap(cropRect.Width, cropRect.Height))
    {
        // Create a new SKCanvas from the cropped bitmap
        using (SKCanvas canvas = new SKCanvas(croppedBitmap))
        {
            // Draw the cropped region onto the canvas
            canvas.DrawBitmap(bitmap, cropRect, new SKRect(0, 0, cropRect.Width, cropRect.Height));
        }

        // Save the cropped bitmap to a file
        using (FileStream fileStream = new FileStream("cropped_screenshot.png", FileMode.Create))
        {
            croppedBitmap.Encode(fileStream, SKEncodedImageFormat.Png, 100);
        }
    }
}


vkOrTcNo.SendKeys("Cheese");
System.Console.WriteLine("Page title is: " + driver.Title);
// driver.Quit();
securityCode.SendKeys("Cheese");

string imgDirPath = Directory.GetCurrentDirectory();
string imgPath = Path.Combine(imgDirPath, "ocr.png");


string ocr = ConvertPythonOcr(imgPath);



Console.WriteLine(ocr);