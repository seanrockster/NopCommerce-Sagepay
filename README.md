NopCommerce Sagepay Plugin
===================

More information about how the plugin works can be found here:

[http://carlosmartinezt.com/2013/11/sagepay-server-integration-with-nopcommerce-3-10-source-code-available/](http://carlosmartinezt.com/2013/11/sagepay-server-integration-with-nopcommerce-3-10-source-code-available)


<a href='https://pledgie.com/campaigns/24757'><img alt='Click here to lend your support to: Sagepay Plugin and make a donation at pledgie.com !' src='https://pledgie.com/campaigns/24757.png?skin_name=chrome' border='0' ></a>


Sage Pay payment plugin for NopCommerce 3.x
-------------------------------------------

Sagepay server is an integration method that is a bit different from other standard methods. It is not a simple redirection method like Paypal standard, nor a direct full API integration. It is an option in the middle because it uses an <strong>iframe</strong> that is displayed inside your payment pages therefore users never leave your website and you don't have to write all the code to validate credit cards and its complexity.

This blog post is intended to help anyone to integrate Sagepay Server with any other eCommerce platform. However, I am using NopCommerce 3.10 as an example on how this was achieved with a NopCommerce payment plugin. I recently developed a Sagepay payment plugin for nopCommerce 3.10 which can be downloaded from: <a href="http://www.nopcommerce.com/p/1211/sagepay-server-payment-plugin-iframe-redirect-modes.aspx">http://www.nopcommerce.com/p/1211/sagepay-server-payment-plugin-iframe-redirect-modes.aspx</a>

Developing a plugin in NopCommerce for me is an opportunity give back to their awesome community. It is fairly easy to create a new plugin and shouldn't be something to scare you off. The quickest way is by copying an existing plugin and modifying it.<!--more-->

**IMPORTANT**

Before starting, this plugin does not work in localhost because Sagepay requires internet access to your website. In this post you will see the URL being nopcommerce.appnovadev.co.uk because it is a publicly accessible URL.

1. INSTALLATION
---------------

The download includes both binary files and source code in case you want to modify something.

<a href="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-plugin-files.jpg"><img class="alignnone size-full wp-image-287" alt="sagepay-plugin-files" src="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-plugin-files.jpg" width="604" height="85" /></a>

Like with any other nopCommerce plugin you only need to drop the binary folder into <strong>Nop.Web/Plugins</strong> folder in nopCommerce. If you are using the source code, then copy the source code folder and paste it into <strong>/Plugins</strong> then go to Visual Studio and <strong>Add an existing project</strong> so that it appears next to the other plugins/

Now go to your nopCommerce installation and go to <strong>Admin &gt; Configuration &gt; Plugins</strong> and install the plugin

<a href="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-plugin-install11.jpg"><img class="alignnone size-full wp-image-297" alt="sagepay-plugin-install1" src="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-plugin-install11.jpg" width="700" height="500" /></a>

It will take approximately 20 seconds to install because the application needs to restart. Once it has successfully installed, go to <strong>Admin &gt; Configuration &gt; Payment Methods</strong> and you will see Sagepay installed in there.

<strong>2. CONFIGURATION</strong>

Click on <strong>configure</strong> to set up your integration:

<a href="http://carlosmartinezt.com/wp-content/uploads/2013/11/sage-pay-configure.jpg"><img class="alignnone size-full wp-image-291" alt="sage-pay-configure" src="http://carlosmartinezt.com/wp-content/uploads/2013/11/sage-pay-configure.jpg" width="700" height="500" /></a>

Transact Type: how to take the money when an order is placed?
<ul>
    <li><strong>Payment</strong> will take the money exactly at the same time when the order is placed. Most merchants use this option</li>
	<li><strong>Deferred</strong> will NOT take the money when the order is placed. It will take the money with the order is <strong>Shipped</strong>. In some countries it is required by law that business should only own the money when the order is shipped. Before the item is shipped, the funds are held by Sagepay as (<strong>awaiting Release or Abort</strong>) but only for a specific number of days. More details can be found in the Sagepay documentation</li>
	<li><strong>Authenticate</strong> this option can be used if you want to validate that the user has a valid credit/debit card so that payments can be made in the future</li>
</ul>
Connect to: here's where you can choose to go live or make test payments
<ul>
	<li><strong>Simulator</strong>: Sagepay offers this feature as a means for developers to test their integrations. It is very easy to create a Sagepay simulator account, just go to <a href="https://support.sagepay.com/apply/simulator/requestAccount">https://support.sagepay.com/apply/simulator/requestAccount</a>. You will be given a <strong>Vendor Name</strong> and basically simulate a real merchant taking online payments. Once you have created an account you can login and see your transactions:</li>
</ul>
<a href="http://carlosmartinezt.com/wp-content/uploads/2013/11/sage-pay-simulator-login.jpg"><img class="alignnone size-full wp-image-292" alt="sage-pay-simulator-login" src="http://carlosmartinezt.com/wp-content/uploads/2013/11/sage-pay-simulator-login.jpg" width="700" height="500" /></a>

You will get the following screen:

<a href="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-simulator-initial-page.jpg"><img class="alignnone size-full wp-image-293" alt="sagepay-simulator-initial-page" src="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-simulator-initial-page.jpg" width="700" height="696" /></a>

Clicking on transactions will show you the list of payments and deferred payments made through the simulator. REMEMBER that Sage pay simulator requires you to add your IP address.

<a href="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-simulator-transactions.jpg"><img class="alignnone size-full wp-image-294" alt="sagepay-simulator-transactions" src="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-simulator-transactions.jpg" width="700" height="696" /></a>
<ul>
	<li><strong>Test</strong>: use this option with the real merchant's Sagepay account. It is required by Sagepay to make a successful test transaction before going live.</li>
	<li><strong>Live</strong>: use this option only when you are ready to start taking payments.</li>
</ul>
Vendor Name: This is the name that identifies your account with Sagepay

Additional Fee: The amount that you enter here will be added to the total of all orders

Profile: Sagepay Server offers two ways to display their payment forms:
<ul>
	<li><strong>LOW</strong>: it shows the payment form without header and footer. Use this option to display the payment forms within an <strong>iframe</strong></li>
	<li><strong>NORMAL</strong>: it shows header and footer. Use this option to <strong>redirect</strong> to a separate page</li>
</ul>
Partner Id: (optional) Only If you have an affiliate account with Sagepay

<strong>3. SEE IT WORKING</strong>

One Sagepay server is set up you are ready to test an order. For now, we will test with the simulator to make sure all the integration is correct and I will be using the LOW profile to show how it looks on an iframe. Go to the front end (public store), add an item to the shopping card and check out. Follow the steps in the checkout process until you get the <strong>payment form</strong> page. You will see the simulator:

<a href="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-simulator-payment-info.jpg"><img class="alignnone  wp-image-295" alt="sagepay-simulator-payment-info" src="http://carlosmartinezt.com/wp-content/uploads/2013/11/sagepay-simulator-payment-info.jpg" width="700" /></a>

Follow the instructions of the simulator. Try to test successful and failed payments. You will see all the messages being transferred between your website and Sagepay.

<strong>4. HOW IT WORKS (Technically)</strong>

There are a few things happening behind the scenes. This plugin creates a new table in NopCommerce called SagePayServerTransaction that is used to store all the information received from Sagepay when registering a new transaction and a reference to the order unique identifier.

This is how Sagepay communicates with Your site (simplifying it a lot):
<ul>
	<li><span style="text-decoration: underline;">Before showing iframe</span>: In paymentInfo page Your site requests a new transaction from Sagepay. Sagepay returns a transaction Id, and a URL that Your site will use to display inside the iframe</li>
	<li><span style="text-decoration: underline;">Inside the iframe (which happens in Sagepay):</span> the user enters their payment details and presses Continue which could result in a successful or failed transactions. Sagepay contacts Your site so that you can verify that transaction is truthful and was originated by you (or if a malicious software is trying to intercept your payments). Here, Sagepay requests a URL like <strong>http://www.mysite.com/Plugins/PaymentSagePayServer/NotificationPage</strong>. Here is where Sagepay needs internet access to your site. If your notification page replies with an OK message, then a payment is made as <strong>Deferred</strong>. It is created as deferred because the order has not yet been confirmed in NopCommerce.</li>
	<li><span style="text-decoration: underline;">Still inside the iframe:</span> Sagepay redirects the user to Your site (response page). Your site automatically posts back to the parent page to get out of the iframe and advances to the confirmation page</li>
	<li><span style="text-decoration: underline;">In Confirmation page:</span> user confirms the page. Order is placed. If you selected TransactType PAYMENT, Your site will contact Sagepay again to <strong>Capture</strong> the payment that was previously deferred and the order payment status is changed to <strong>Paid</strong>. If you selected TransactType DEFERRED you need to ship the order to capture the payment at a later stage</li>
</ul>
