package com.nui.android.activities;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.support.v4.content.ContextCompat;
import android.support.v4.view.GestureDetectorCompat;
import android.os.Bundle;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.ScaleGestureDetector;
import android.view.View;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.ImageView;

import com.nui.android.AccelerometerMonitor;
import com.nui.android.Network;
import com.nui.android.PinchGestureListener;
import com.nui.android.R;
import com.nui.android.RotationMonitor;
import com.nui.android.SensorMonitor;
import com.nui.android.Shape;
import com.nui.android.SwipeGestureListener;
import com.nui.android.TouchGestureListener;

import java.net.DatagramPacket;
import java.util.Random;

/**
 * Base activity
 */
public class BaseActivity extends Activity {

    //private Network network;
    GestureDetectorCompat swipeDetector;
    ScaleGestureDetector pinchDetector;
    GestureDetectorCompat touchDetector;
    private AccelerometerMonitor acceloremeterSensor;
    private RotationMonitor rotationSensor;

    public static String shape;
    public static String nextShape;

    private ImageView circleView;
    private ImageView squareView;

    private ImageView pullShape;
    private Button moveCursor;
    private boolean sendGyroData = false;

    private final Random random = new Random();
    private int count;
    private static int MAX_COUNT = 2;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_base);
        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_PORTRAIT);

        getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LAYOUT_STABLE
                | View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
                | View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
                | View.SYSTEM_UI_FLAG_FULLSCREEN
                | View.SYSTEM_UI_FLAG_IMMERSIVE);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);


        initNetwork();
        SwipeGestureListener swipeGestureListener = new SwipeGestureListener(Network.getInstance());
        rotationSensor = new RotationMonitor(Network.getInstance(), this);
        acceloremeterSensor = new AccelerometerMonitor(Network.getInstance(), rotationSensor, this);
        swipeDetector = new GestureDetectorCompat(this, swipeGestureListener);
        pinchDetector = new ScaleGestureDetector(this, new PinchGestureListener(Network.getInstance(), swipeGestureListener));
        touchDetector = new GestureDetectorCompat(this, new TouchGestureListener(this, Network.getInstance()));

        circleView = (ImageView) findViewById(R.id.circle);
        squareView = (ImageView) findViewById(R.id.square);
        pullShape = (ImageView) findViewById(R.id.pull_shape);
        pullShape.setVisibility(View.INVISIBLE);
        circleView.setVisibility(View.INVISIBLE);
        squareView.setVisibility(View.INVISIBLE);
        count = 0;
        moveCursor = (Button) findViewById(R.id.move_cursor);

        moveCursor.setOnTouchListener(new View.OnTouchListener() {
            public boolean onTouch(View view, MotionEvent event) {
                if (event.getAction() == android.view.MotionEvent.ACTION_DOWN) {
                    Log.d("TouchTest", "Touch down");
                    sendGyroData = true;
                    moveCursor.setBackgroundColor(ContextCompat.getColor(moveCursor.getContext(), R.color.colorDarkGrey));
                    return true;
                } else if (event.getAction() == android.view.MotionEvent.ACTION_UP) {
                    Log.d("TouchTest", "Touch up");
                    moveCursor.setBackgroundColor(ContextCompat.getColor(moveCursor.getContext(), R.color.colorLightGrey));
                    sendGyroData = false;
                    return true;
                }
                return false;
            }
        });

        sm = (SensorManager) getSystemService(SENSOR_SERVICE);
        // TODO provide support for gyroscope (rotation vector is flawed in early
        // versions of android)
        rv = sm.getDefaultSensor(Sensor.TYPE_GYROSCOPE);

        // network thread
        nt = new Thread(new Runnable() {
            @Override
            public void run() {
                long lt = 0;

                while (true) {
                    if (end_nt) {
                        Log.d("BaseActivity", "Network thread ends.");
                        break;
                    }

                    if (sendGyroData && rv_sel.getLatestTimestamp() > lt) {
                        try {
                            Network.getInstance().ds.send(dp);
                            lt = rv_sel.getLatestTimestamp();
                        } catch (Exception e) {
                            e.printStackTrace();
                        }
                    }
                }
            }
        }, "UdpThread");

        nt.setPriority(Thread.MIN_PRIORITY);
        if(Network.getInstance().ds != null) {
            nt.start();
        }else {
            Network.getInstance().Reconnect();
        }
        // TODO rewrite the sensor acquisition with NDK
        rv_sel = new RotationVectorListener();

    }

    private boolean pushOrPull;

    public boolean PushOrPull(){
        //True = push, false = pull
        return pushOrPull;
    }

    protected void initNetwork()
    {
        Network.initInstance(this);
    }

    public void StartPullTest(){
        pushOrPull = false;
        circleView.setVisibility(View.INVISIBLE);
        squareView.setVisibility(View.INVISIBLE);
        pullShape.setVisibility(View.VISIBLE);

        pullShape.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {

                touchDetector.onTouchEvent(event);
                swipeDetector.onTouchEvent(event);
                pinchDetector.onTouchEvent(event);

                return true;
            }

        });
    }

    public void SetGesture(String gesture){
        switch (gesture){
            case "tilt": case "throw": acceloremeterSensor.SetTiltorThrow(gesture); break;
            default: break;
        }
    }

    public void StartPushTest(){

        pushOrPull = true;
        pullShape.setVisibility(View.INVISIBLE);
        circleView.setVisibility(View.VISIBLE);
        squareView.setVisibility(View.VISIBLE);

        circleView.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                shape = Shape.Circle;

                touchDetector.onTouchEvent(event);
                swipeDetector.onTouchEvent(event);
                pinchDetector.onTouchEvent(event);

                if(event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE) {
                    circleView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.circle_stroke));
                    squareView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.square));
                }

                return true;
            }

        });

        squareView.setOnTouchListener(new View.OnTouchListener() {
            @Override
            public boolean onTouch(View v, MotionEvent event) {
                shape = Shape.Square;

                touchDetector.onTouchEvent(event);
                swipeDetector.onTouchEvent(event);
                pinchDetector.onTouchEvent(event);

                if(event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE) {
                    squareView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.square_stroke));
                    circleView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.circle));
                }

                return true;
            }

        });
    }

    boolean pullPinchWaiting = false;
    public void AwaitingPullPinch(boolean waiting){
        pullPinchWaiting = waiting;
    }

    public boolean IsWaitingForPinch(){
        return pullPinchWaiting;
    }

    public void ClearShapes(){
        shape = null;
        squareView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.square));
        circleView.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.circle));
    }

    public void SwitchPosition() {
        ClearShapes();
        if(count > MAX_COUNT || random.nextBoolean()) {
            count = 0;
            int TopShapeTop = circleView.getTop();
            int TopShapeBottom = circleView.getBottom();
            int BottomShapeTop = squareView.getTop();
            int BottomShapeBottom = squareView.getBottom();

            circleView.setTop(BottomShapeTop);
            circleView.setBottom(BottomShapeBottom);
            squareView.setTop(TopShapeTop);
            squareView.setBottom(TopShapeBottom);
        } else {
            count++;
        }
    }

    public void SetShape(String shape) {
        ClearShapes();
        shape = shape;

        if(shape.equals("circle")) {
            pullShape.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.circle));
        }
        else {
            pullShape.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.square));
        }
    }

    public boolean ReadyToStart(){
        return true;
    }

    public static String GetSelectedShape(){
        return shape;
    }

    public void CloseApp(){
        this.finish();
        Intent intent = new Intent(Intent.ACTION_MAIN);
        intent.addCategory(Intent.CATEGORY_HOME);
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        startActivity(intent);
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {

        menu.add(Menu.NONE, R.id.network_discovery, Menu.NONE, R.string.network_discovery);
        menu.add(Menu.NONE, R.id.reconnect_action, Menu.NONE, R.string.reconnect_action);
        menu.add(Menu.NONE, R.id.close_app_action, Menu.NONE, R.string.close_app_action);

        return super.onCreateOptionsMenu(menu);
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.network_discovery:
                Network.getInstance().FindServer(true);
                return true;
            case R.id.reconnect_action:
                Network.getInstance().Reconnect();
                return true;
            case R.id.close_app_action:
                //android.os.Process.killProcess(android.os.Process.myPid());
                CloseApp();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    @Override
    protected void onPause(){
        super.onPause();
        acceloremeterSensor.Pause();
        sm.unregisterListener(rv_sel);
        Network.getInstance().Pause();
    }

    @Override
    protected void onResume(){
        super.onResume();
        Network.getInstance().Resume();
        acceloremeterSensor.Resume();
        sm.registerListener(rv_sel, rv, SensorManager.SENSOR_DELAY_GAME);
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        if (nt.isAlive()) {
            end_nt = true;
        }
    }

    @Override
    protected void onStop(){
        super.onStop();
    }

    @Override
    public void onBackPressed() {
        super.onBackPressed();
    }

    private SensorManager sm;
    private Sensor rv;
    private RotationVectorListener rv_sel;
    private byte[] msg = new byte[100];
    public DatagramPacket dp = new DatagramPacket(msg, msg.length);
    public Thread nt;
    private boolean end_nt;

    class RotationVectorListener implements SensorEventListener {
        private long time = 0;
        public boolean calibrated = false;
        private float calibrateZ = 0;
        private float calibrateX = 0;
        private float calibrateY = 0;

        private float virtualX = 0;
        private float virtualY = 0;
        private float virtualZ = 0;
//		private String info_text;

        public long getLatestTimestamp() {
            return time;
        }

        @Override
        public void onSensorChanged(SensorEvent event) {
            if(time == 0)
                time = event.timestamp;

            float x = event.values[0];
            float y = event.values[1];
            float z = event.values[2];

            if(!calibrated){
                calibrateZ = z;
                calibrateX = x;
                calibrateY = y;
                calibrated = true;
            }

            virtualX = x-calibrateX;
            virtualY = y-calibrateY;
            virtualZ = z-calibrateZ;

            //Log.d("Gyro: ", "X: " + x + " Y: " + y + " Z: " + z);
            byte[] buf = ("gyrodata:time:"+ event.timestamp +":x:"+x+":y:"+y+":z:"+z).getBytes();
            dp.setData(buf);
            time = event.timestamp;
        }

        @Override
        public void onAccuracyChanged(Sensor sensor, int accuracy) {
            // TODO Auto-generated method stub

        }
    }

}
